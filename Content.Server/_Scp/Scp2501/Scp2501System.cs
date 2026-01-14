using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Serialization;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Scp.Scp2501;

[Serializable, NetSerializable]
public sealed partial class Scp2501CrushDoAfterEvent : DoAfterEvent 
{ 
    public override DoAfterEvent Clone() => this; 
}

public sealed class Scp2501System : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Scp2501Component, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<Scp2501Component, Scp2501CrushDoAfterEvent>(OnCrushFinished);
    }

    private void OnAfterInteract(EntityUid uid, Scp2501Component comp, AfterInteractEvent args)
    {
        if (args.Target == null || args.Handled || !Exists(args.Target.Value))
            return;

        StartCrush(uid, comp, args.User, args.Target.Value);
        args.Handled = true;
    }

    private void StartCrush(EntityUid uid, Scp2501Component comp, EntityUid user, EntityUid target)
    {
        if (comp.IsCrushing) return;

        if (!_interaction.InRangeUnobstructed(user, target, comp.Range))
            return;

        var ev = new Scp2501CrushDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, user, comp.CrushDelay, ev, uid, target: target, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
            DistanceThreshold = 100f
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
        {
            comp.IsCrushing = true;
            if (comp.AudioStream == null)
                comp.AudioStream = _audio.PlayPvs(comp.SoundCrushing, uid)?.Entity;
        }
    }

    private void OnCrushFinished(EntityUid uid, Scp2501Component comp, Scp2501CrushDoAfterEvent args)
    {
        comp.IsCrushing = false;

        if (args.Cancelled || args.Target == null || !Exists(args.Target.Value))
        {
            StopCrushEffects(comp);
            return;
        }

        var target = args.Target.Value;
        var user = args.User;

        // это нужно чтобы не было набегаторов использующих 2501 (с ним можно очень быстро гибнуть кого то)
        if (TryComp<MobStateComponent>(target, out var mobState))
        {
            if (_mobState.IsCritical(target, mobState) || _mobState.IsDead(target, mobState))
            {
                _popup.PopupEntity("Лопасти клешни издают пронзительный скрежет и останавливаются", user, user, PopupType.MediumCaution);
                StopCrushEffects(comp);
                return;
            }
        }

        _audio.PlayPvs(comp.SoundFinish, target);
        _damageable.TryChangeDamage(target, comp.Damage, ignoreResistances: true);
        
        _popup.PopupEntity("ХРУСТ!", target, PopupType.LargeCaution);

        if (Exists(target) && !Deleted(target))
        {
            StartCrush(uid, comp, user, target);
        }
        else
        {
            StopCrushEffects(comp);
        }
    }

    private void StopCrushEffects(Scp2501Component comp)
    {
        if (comp.AudioStream != null)
        {
            _audio.Stop(comp.AudioStream);
            comp.AudioStream = null;
        }
    }
}