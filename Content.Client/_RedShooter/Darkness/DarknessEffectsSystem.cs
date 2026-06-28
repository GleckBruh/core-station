using Content.Shared._RedShooter.Darkness.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Client._RedShooter.Darkness;

public sealed class DarknessEffectsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private DarknessVignetteOverlay? _overlay;

    public float TargetExposureFraction { get; private set; }
    public float VisualExposureFraction { get; private set; }

    private Entity<AudioComponent>? _darknessLoopStream;

    private readonly SoundSpecifier _darknessLoopSound =
        new SoundPathSpecifier("/Audio/_RedShooter/Darkness/murk_loop.ogg");

    private const float SoundStartThreshold = 0.12f;
    private const float SoundStopThreshold = 0.03f;

    private const float MinLoopVolume = -18f;
    private const float MaxLoopVolume = -7f;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new DarknessVignetteOverlay(this);
        _overlayManager.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_overlay != null)
        {
            _overlayManager.RemoveOverlay(_overlay);
            _overlay = null;
        }

        StopDarknessLoop();
    }

    private void UpdateDarknessLoop()
    {
        var visual = VisualExposureFraction;
        var target = TargetExposureFraction;

        if (_darknessLoopStream == null)
        {
            if (visual < SoundStartThreshold)
                return;

            StartDarknessLoop();
        }

        if (_darknessLoopStream == null)
            return;

        // Если реальный Exposure уже почти ушёл, и визуальный хвост тоже затих —
        // выключаем звук.
        if (target <= 0.01f && visual <= SoundStopThreshold)
        {
            StopDarknessLoop();
            return;
        }

        var volumeFraction = Math.Clamp(
            (visual - SoundStartThreshold) / (1f - SoundStartThreshold),
            0f,
            1f);

        volumeFraction = MathF.Sqrt(volumeFraction);

        var volume = MathHelper.Lerp(MinLoopVolume, MaxLoopVolume, volumeFraction);

        _audio.SetVolume(_darknessLoopStream.Value, volume);
    }

    private void StartDarknessLoop()
    {
        _darknessLoopStream = _audio.PlayGlobal(
            _darknessLoopSound,
            Filter.Local(),
            false,
            AudioParams.Default
                .WithLoop(true)
                .WithVolume(MinLoopVolume));
    }

    private void StopDarknessLoop()
    {
        if (_darknessLoopStream == null)
            return;

        var stream = _darknessLoopStream.Value;

        _audio.Stop(stream);

        if (!Deleted(stream.Owner))
            QueueDel(stream.Owner);

        _darknessLoopStream = null;
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        TargetExposureFraction = GetLocalExposureFraction();

        // Нарастание быстрее, спад медленнее.
        // Это именно плавное движение каждый кадр, а не Lerp к ступенькам сервера.
        var speed = TargetExposureFraction > VisualExposureFraction
            ? 0.9f
            : 0.45f;

        VisualExposureFraction = MoveTowards(
            VisualExposureFraction,
            TargetExposureFraction,
            speed * frameTime);

        if (TargetExposureFraction <= 0.001f && VisualExposureFraction <= 0.01f)
            VisualExposureFraction = 0f;

        UpdateDarknessLoop();
    }
    private float GetLocalExposureFraction()
    {
        var localEntity = _player.LocalEntity;

        if (localEntity == null)
            return 0f;

        if (!TryComp<DarknessVulnerableComponent>(localEntity.Value, out var darkness))
            return 0f;

        if (darkness.MaxExposure <= 0f)
            return 0f;

        return Math.Clamp(darkness.Exposure / darkness.MaxExposure, 0f, 1f);
    }
}
