using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Maths;

namespace Content.Client._RedShooter.Darkness;

public sealed class DarknessVignetteOverlay : Overlay
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly DarknessEffectsSystem _system;
    private readonly Texture _vignette;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public DarknessVignetteOverlay(DarknessEffectsSystem system)
    {
        IoCManager.InjectDependencies(this);

        _system = system;

        _vignette = _resourceCache.GetResource<TextureResource>(
            "/Textures/_RedShooter/Darkness/murk_vignette.png").Texture;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var exposure = _system.VisualExposureFraction;

        if (exposure <= 0.01f)
            return;

        var handle = args.ScreenHandle;
        var viewport = args.ViewportBounds;

        var power = MathF.Pow(exposure, 1.15f);

        var alpha = Math.Clamp(power * 3f, 0f, 0.9f);

        // Сжатие теперь намного слабее.
        // 1.35f при слабом Мраке, 1.08f при сильном.
        // То есть виньетка не схлопывается прямо в центр экрана.
        var scale = MathHelper.Lerp(1.35f, 1.08f, power);

        var center = viewport.Center;
        var size = viewport.Size * scale;
        var rect = UIBox2.FromDimensions(center - size / 2f, size);

        handle.DrawTextureRect(_vignette, rect, Color.White.WithAlpha(alpha));
    }
}
