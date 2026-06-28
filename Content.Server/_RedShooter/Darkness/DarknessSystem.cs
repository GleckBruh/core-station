using Content.Shared._RedShooter.Darkness.Components;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Content.Shared.Interaction;
using Content.Shared.Hands.Components;
using Robust.Server.GameObjects;
using Content.Server.Light.Components;
using Content.Server.Popups;
using Content.Shared.Light.Components;
using Robust.Shared.Random;

namespace Content.Server._RedShooter.Darkness;

public sealed class DarknessSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private float _accumulator;

    // Не каждый тик. Для такой механики 1 секунды достаточно.
    private const float UpdateInterval = 1f;



    private (DamageSpecifier Damage, TimeSpan Interval)? GetDamageStage(DarknessVulnerableComponent vulnerable)
    {
        if (vulnerable.MaxExposure <= 0f)
            return null;

        var exposurePercent = vulnerable.Exposure / vulnerable.MaxExposure * 100f;

        if (exposurePercent >= vulnerable.HeavyDamageThreshold)
            return (vulnerable.HeavyDamage, vulnerable.HeavyDamageInterval);

        if (exposurePercent >= vulnerable.MediumDamageThreshold)
            return (vulnerable.MediumDamage, vulnerable.MediumDamageInterval);

        if (exposurePercent >= vulnerable.LightDamageThreshold)
            return (vulnerable.LightDamage, vulnerable.LightDamageInterval);

        return null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;

        if (_accumulator < UpdateInterval)
            return;

        var delta = _accumulator;
        _accumulator = 0f;

        if (!_cfg.GetCVar(CCVars.DarknessEnabled))
            return;

        var query = EntityQueryEnumerator<DarknessVulnerableComponent>();

        while (query.MoveNext(out var uid, out var vulnerable))
        {
            if (_mobState.IsDead(uid))
                continue;

            ProcessDarkness(uid, vulnerable, delta);
        }
    }

    private void ProcessDarkness(EntityUid uid, DarknessVulnerableComponent vulnerable, float delta)
    {
        var inDarkness = IsInDarkness(uid);
        var protectedFromDarkness = IsProtectedFromDarkness(uid);

        if (!inDarkness || protectedFromDarkness)
        {
            DecayExposure(uid, vulnerable, delta);
            return;
        }

        GainExposure(uid, vulnerable, delta);

        TryShowDarknessWarning(uid, vulnerable);

        var stage = GetDamageStage(vulnerable);

        if (stage == null)
            return;

        var (damage, interval) = stage.Value;

        if (_timing.CurTime < vulnerable.NextDamageTime)
            return;

        _damageable.TryChangeDamage(uid, damage, true, false);
        _audio.PlayPvs(vulnerable.DamageSound, uid);

        vulnerable.NextDamageTime = _timing.CurTime + interval;
    }

    private static readonly string[] DarknessWarningMessages =
    {
        "darkness-warning-pressure",
        "darkness-warning-breath",
        "darkness-warning-skin",
        "darkness-warning-eyes",
        "darkness-warning-whisper",
    };

    private void TryShowDarknessWarning(EntityUid uid, DarknessVulnerableComponent vulnerable)
    {
        if (vulnerable.MaxExposure <= 0f)
            return;

        var exposureFraction = vulnerable.Exposure / vulnerable.MaxExposure;

        if (exposureFraction < vulnerable.WarningExposureFraction)
            return;

        if (_timing.CurTime < vulnerable.NextWarningTime)
            return;

        var message = Loc.GetString(_random.Pick(DarknessWarningMessages));
        _popup.PopupEntity(message, uid, uid);

        var randomOffset = TimeSpan.FromSeconds(_random.NextFloat(-3f, 4f));

        vulnerable.NextWarningTime = _timing.CurTime
                                     + vulnerable.WarningInterval
                                     + randomOffset;
    }

    private void GainExposure(EntityUid uid, DarknessVulnerableComponent vulnerable, float delta)
    {
        var old = vulnerable.Exposure;

        vulnerable.Exposure = Math.Clamp(
            vulnerable.Exposure + vulnerable.ExposureGainPerSecond * delta,
            0f,
            vulnerable.MaxExposure);

        if (Math.Abs(old - vulnerable.Exposure) > 0.001f)
            Dirty(uid, vulnerable);
    }

    private void DecayExposure(EntityUid uid, DarknessVulnerableComponent vulnerable, float delta)
    {
        if (vulnerable.Exposure <= 0f)
            return;

        var old = vulnerable.Exposure;

        vulnerable.Exposure = Math.Clamp(
            vulnerable.Exposure - vulnerable.ExposureDecayPerSecond * delta,
            0f,
            vulnerable.MaxExposure);

        if (Math.Abs(old - vulnerable.Exposure) > 0.001f)
            Dirty(uid, vulnerable);
    }

    /// <summary>
    /// Проверяет только то, существует ли Мрак в этой точке.
    /// Не проверяет свет, скафандры и иммунитет.
    /// </summary>
    public bool IsInDarkness(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var xform))
            return false;

        // 1. Suppressor всегда побеждает.
        if (IsSuppressedByMarker(uid, xform))
            return false;

        // 2. Если grid помечен как DarknessMap — Мрак есть по умолчанию.
        if (xform.GridUid is { } gridUid &&
            TryComp<DarknessMapComponent>(gridUid, out var darknessMap) &&
            darknessMap.Enabled)
        {
            return true;
        }

        // 3. Если grid не тёмный, локальный DarknessZone всё равно может создать Мрак.
        if (IsInsideDarknessZone(uid, xform))
            return true;

        return false;
    }

    /// <summary>
    /// Проверяет защиту от Мрака.
    /// Эту функцию потом сможет использовать тренировочный ошейник.
    /// </summary>
    public bool IsProtectedFromDarkness(EntityUid uid)
    {
        if (HasComp<DarknessImmuneComponent>(uid))
            return true;

        if (HasEquipmentProtection(uid))
            return true;

        if (IsProtectedBySource(uid))
            return true;

        return false;
    }

    private bool IsSourceExposed(EntityUid sourceUid)
    {
        // Если объект не в контейнере — он лежит в мире.
        if (!_container.IsEntityInContainer(sourceUid))
            return true;

        if (!TryComp<TransformComponent>(sourceUid, out var xform))
            return false;

        var parent = xform.ParentUid;

        // Предмет лежит прямо в руках существа.
        if (HasComp<HandsComponent>(parent))
            return true;

        // Предмет лежит прямо в inventory-слоте существа:
        // карман, пояс, suit storage и подобное.
        if (HasComp<InventoryComponent>(parent))
            return true;

        // Всё остальное считаем закрытым или вложенным контейнером:
        // рюкзак, коробка, ящик, шкаф и т.п.
        return false;
    }

    private bool TryGetExpendableLightProtectionRadius(
        DarknessProtectionSourceComponent source,
        ExpendableLightComponent expendable,
        out float radius)
    {
        radius = 0f;

        switch (expendable.CurrentState)
        {
            case ExpendableLightState.Lit:
                radius = source.Radius;
                return radius > source.MinimumProtectionRadius;

            case ExpendableLightState.Fading:
            {
                var fadeSeconds = (float) expendable.FadeOutDuration.TotalSeconds;

                if (fadeSeconds <= 0f)
                    return false;

                var fadeFraction = Math.Clamp(expendable.StateExpiryTime / fadeSeconds, 0f, 1f);

                if (fadeFraction <= source.MinimumExpendableFadeFraction)
                    return false;

                radius = source.ScaleWithExpendableLightFade
                    ? source.Radius * fadeFraction
                    : source.Radius;

                return radius > source.MinimumProtectionRadius;
            }

            case ExpendableLightState.BrandNew:
            case ExpendableLightState.Dead:
            default:
                return false;
        }
    }

    private bool TryGetEffectiveProtectionRadius(
        EntityUid sourceUid,
        DarknessProtectionSourceComponent source,
        out float radius)
    {
        radius = 0f;

        if (!source.Enabled)
            return false;

        // ExpendableLight проверяем отдельно.
        // Для него PointLight не является источником истины.
        if (TryComp<ExpendableLightComponent>(sourceUid, out var expendable))
            return TryGetExpendableLightProtectionRadius(source, expendable, out radius);

        radius = source.Radius;

        if (!source.RequiresActiveLight)
            return radius > source.MinimumProtectionRadius;

        if (!TryComp<PointLightComponent>(sourceUid, out var light))
            return false;

        if (!light.Enabled)
            return false;

        return radius > source.MinimumProtectionRadius;
    }

    private bool HasEquipmentProtection(EntityUid uid)
    {
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return false;

        if (!_inventory.TryGetContainerSlotEnumerator((uid, inventory), out var enumerator))
            return false;

        while (enumerator.NextItem(out var item))
        {
            if (!TryComp<DarknessProtectionEquipmentComponent>(item, out var protection))
                continue;

            if (!protection.Enabled)
                continue;

            return true;
        }

        return false;
    }

    private bool IsSuppressedByMarker(EntityUid uid, TransformComponent xform)
    {
        var query = EntityQueryEnumerator<DarknessSuppressorComponent, TransformComponent>();

        while (query.MoveNext(out var markerUid, out var suppressor, out var markerXform))
        {
            if (!suppressor.Enabled)
                continue;

            if (!SameMap(xform, markerXform))
                continue;

            if (!TryDistance(uid, markerUid, xform, markerXform, out var distance))
                continue;

            if (distance <= suppressor.Radius)
                return true;
        }

        return false;
    }

    private bool IsInsideDarknessZone(EntityUid uid, TransformComponent xform)
    {
        var query = EntityQueryEnumerator<DarknessZoneComponent, TransformComponent>();

        while (query.MoveNext(out var markerUid, out var zone, out var markerXform))
        {
            if (!zone.Enabled)
                continue;

            if (!SameMap(xform, markerXform))
                continue;

            if (!TryDistance(uid, markerUid, xform, markerXform, out var distance))
                continue;

            if (distance <= zone.Radius)
                return true;
        }

        return false;
    }

    private bool IsProtectedBySource(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var xform))
            return false;

        var query = EntityQueryEnumerator<DarknessProtectionSourceComponent, TransformComponent>();

        while (query.MoveNext(out var sourceUid, out var source, out var sourceXform))
        {
            if (!TryGetEffectiveProtectionRadius(sourceUid, source, out var protectionRadius))
                continue;

            if (!IsSourceExposed(sourceUid))
                continue;

            if (!SameMap(xform, sourceXform))
                continue;

            if (!TryDistance(uid, sourceUid, xform, sourceXform, out var distance))
                continue;

            if (distance > protectionRadius)
                continue;

            if (!_interaction.InRangeUnobstructed(uid, sourceUid, protectionRadius))
                continue;

            return true;
        }

        return false;
    }

    private bool SameMap(TransformComponent a, TransformComponent b)
    {
        return a.MapID == b.MapID;
    }

    private bool TryDistance(
        EntityUid a,
        EntityUid b,
        TransformComponent aXform,
        TransformComponent bXform,
        out float distance)
    {
        return aXform.Coordinates.TryDistance(EntityManager, _transform, bXform.Coordinates, out distance);
    }
}
