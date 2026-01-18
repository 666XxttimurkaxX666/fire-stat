using Content.Shared._Scp.Scp077;
using Robust.Client.GameObjects;

namespace Content.Client._Scp.Scp077;

public sealed class Scp077VisualizerSystem : VisualizerSystem<Scp077Component>
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void OnAppearanceChange(EntityUid uid, Scp077Component component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null) return;

        // 1. Основной спрайт черепа
        if (AppearanceSystem.TryGetData<int>(uid, Scp077Visuals.RuneState, out var state, args.Component))
        {
            args.Sprite.LayerSetState(0, $"scp_077_r_{state}");
        }

        // 2. Управление ОВЕРЛЕЕМ (Слой 1)
        bool isVisible = false;
        AppearanceSystem.TryGetData<bool>(uid, Scp077Visuals.GlowVisible, out isVisible, args.Component);
        
        args.Sprite.LayerSetVisible(1, isVisible);

        if (isVisible && AppearanceSystem.TryGetData<Scp077GlowType>(uid, Scp077Visuals.GlowColor, out var color, args.Component))
        {
            var stateName = color == Scp077GlowType.Blue ? "scp_077_blue" : "scp_077_green";
            args.Sprite.LayerSetState(1, stateName);
            // Делаем глаза светящимися (игнорируют освещение)
            args.Sprite.LayerSetShader(1, "unshaded"); 
        }
    }
}