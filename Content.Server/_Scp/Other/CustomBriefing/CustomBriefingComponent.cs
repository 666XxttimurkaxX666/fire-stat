using Robust.Shared.Audio;

namespace Content.Server._Scp.Other.CustomBriefing;

[RegisterComponent, Access(typeof(CustomBriefingSystem))]
public sealed partial class CustomBriefingComponent : Component
{
    [DataField]
    public SoundSpecifier? BriefingSound;

    [DataField]
    public string BriefingText;

    [DataField]
    public string? BriefingColor;

    [ViewVariables]
    public bool IsIssued = false;
}
