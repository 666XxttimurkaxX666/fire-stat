using Content.Shared._Scp.Scp077;
using Content.Shared.Examine;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Jittering;
using Content.Server.Chat.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Text;
using System.Linq;

namespace Content.Server._Scp.Scp077;

public sealed class Scp077System : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Scp077Component, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<Scp077Component, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, Scp077Component component, ComponentStartup args)
    {
        component.CurrentTimer = component.TimerInterval;
        GenerateNewRitual(uid, component);
    }

    private void GenerateNewRitual(EntityUid uid, Scp077Component component)
    {
        var phraseCount = _random.Next(5, 9);
        var words = new List<string>();
        for (int i = 0; i < phraseCount; i++)
        {
            words.Add(_random.Pick(component.RuneNames));
        }
        component.ActivePhrase = string.Join(" ", words);

        var stateIndex = _random.Next(1, 4);
        _appearance.SetData(uid, Scp077Visuals.RuneState, stateIndex);
        
        RemComp<JitteringComponent>(uid);
        component.RitualAttempts.Clear();
    }

    private void OnExamined(EntityUid uid, Scp077Component component, ExaminedEvent args)
    {
        var ratio = component.CurrentTimer / component.TimerInterval;
        var timerColor = ratio < 0.20f ? "darkred" : ratio < 0.5f ? "red" : "white";
        var timerText = ratio < 0.20f ? "яростно пульсируют" : "светятся";

        args.PushMarkup($"Руны на черепе [color={timerColor}][bold]{timerText}[/bold][/color]");

        var words = component.ActivePhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var symbolPhrase = new StringBuilder();
        foreach (var word in words)
        {
            int index = component.RuneNames.IndexOf(word);
            if (index != -1) symbolPhrase.Append($"{component.RuneSymbols[index]}  ");
        }
        args.PushMarkup($"\n[color=lawngreen][font size=16]>> {symbolPhrase} <<[/font][/color]");
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        var cleanMessage = new string(args.Message.ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
        var playerWords = cleanMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();

        var query = EntityQueryEnumerator<Scp077Component, TransformComponent>();
        while (query.MoveNext(out var uid, out var scp, out var xform))
        {
            if (!_transform.InRange(xform.Coordinates, Transform(args.Source).Coordinates, 3f))
                continue;

            var targetWords = scp.ActivePhrase.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            // ПРОВЕРКА ПРАВИЛЬНОЙ ФРАЗЫ (УНИСОН)
            if (playerWords.SequenceEqual(targetWords))
            {
                var currentTime = _timing.CurTime;

                foreach (var (user, time) in scp.RitualAttempts.ToArray())
                {
                    if ((currentTime - time).TotalSeconds > 3.0)
                        scp.RitualAttempts.Remove(user);
                }

                scp.RitualAttempts[args.Source] = currentTime;

                if (scp.RitualAttempts.Count >= 2)
                {
                    // успех
                    scp.CurrentTimer = scp.TimerInterval;
                    scp.BlueGlowTimer = 1.0f; 
                    _appearance.SetData(uid, Scp077Visuals.GlowVisible, true);
                    _appearance.SetData(uid, Scp077Visuals.GlowColor, Scp077GlowType.Blue);
                    
                    _jittering.AddJitter(uid, 10f, 100f); 
                    EntityManager.SpawnEntity("EffectFlashBluespace", xform.Coordinates);

                    GenerateNewRitual(uid, scp);
                }
                else
                {
                    // почему не работает суки
                    _jittering.AddJitter(uid, 1.5f, 10f);
                }
                return;
            }

            // --- НАКАЗАНИЕ ЗА НЕВЕРНУЮ ФРАЗУ ---
            // Проверяем, пытался ли игрок вообще читать руны (содержит ли его речь слова из списка рун)
            bool isRuneAttempt = playerWords.Any(w => scp.RuneNames.Any(r => r.Equals(w, StringComparison.OrdinalIgnoreCase)));
            
            if (isRuneAttempt)
            {
                // нукжно потом другой урон сделать
                var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Blunt"), scp.FailDamage);
                _damageable.TryChangeDamage(args.Source, damage, true);

                // штраф времени (по приколу)
                scp.CurrentTimer -= 20f;

            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<Scp077Component, TransformComponent>();
        while (query.MoveNext(out var uid, out var scp, out var xform))
        {
            scp.CurrentTimer -= frameTime;

            if (scp.BlueGlowTimer > 0)
            {
                scp.BlueGlowTimer -= frameTime;
                if (scp.BlueGlowTimer <= 0)
                    _appearance.SetData(uid, Scp077Visuals.GlowVisible, false);
            }
            else if (scp.CurrentTimer < 120f && scp.CurrentTimer > 0)
            {
                _appearance.SetData(uid, Scp077Visuals.GlowVisible, true);
                _appearance.SetData(uid, Scp077Visuals.GlowColor, Scp077GlowType.Green);
                
                var intensity = 1f + (1f - (scp.CurrentTimer / 120f)) * 4f;
                _jittering.AddJitter(uid, intensity, 5f);
            }
            else if (scp.CurrentTimer > 120f)
            {
                 _appearance.SetData(uid, Scp077Visuals.GlowVisible, false);
                 RemCompDeferred<JitteringComponent>(uid);
            }

            // газ
            if (scp.CurrentTimer <= 0)
            {
                // дрд сделай дым
                EntityManager.SpawnEntity("Scp077GasCloud", xform.Coordinates);
                
                // перезапуск до следующего вброса
                scp.CurrentTimer = 60f; 
            }
        }
    }
}