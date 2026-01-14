using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server._Scp.Scp2501;

[RegisterComponent]
public sealed partial class Scp2501Component : Component
{
    [DataField("damage")] 
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2> 
        { 
            { "Blunt", FixedPoint2.New(80) },
            { "Structural", FixedPoint2.New(100) }
        }
    };

    [DataField("crushDelay")] 
    public float CrushDelay = 2f;

    [DataField("range")]
    public float Range = 10f;

    [DataField("soundCrushing")]
    public SoundSpecifier SoundCrushing = new SoundPathSpecifier("/Audio/Machines/airlock_creaking.ogg");

    [DataField("soundFinish")]
    public SoundSpecifier SoundFinish = new SoundPathSpecifier("/Audio/_Sunrise/Cyborg/robot_legs1.ogg");

    public bool IsCrushing = false;
    public EntityUid? AudioStream;
}