using Content.Server.Antag;
using Content.Shared.Mind.Components;

namespace Content.Server._Scp.Other.CustomBriefing;

public sealed partial class CustomBriefingSystem : EntitySystem
{
    [Dependency] private AntagSelectionSystem _antag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomBriefingComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<CustomBriefingComponent> ent, ref MindAddedMessage args)
    {
        if (ent.Comp.IsIssued)
            return;

        var color = Color.FromHex(ent.Comp.BriefingColor, Color.Gray);

        _antag.SendBriefing(
            ent.Owner,
            Loc.GetString(ent.Comp.BriefingText),
            color,
            ent.Comp.BriefingSound
        );

        ent.Comp.IsIssued = true;
    }
}
