#if DEBUG

using Content.Shared.GameTicking;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.IoC;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server._Core.Planetary.Test;

public sealed class CoreDebugPlanetLoaderSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly LinkedEntitySystem _linked = default!;

    private const string DebugToPlanetPortalProto = "PortalGateDebugToPlanet";
    private const string PlanetToDebugPortalProto = "PortalGatePlanetToDebug";

    private bool _loaded;

    private static readonly ResPath PlanetMapPath =
        new("/Maps/_Core/Test/core_dev_planet.yml");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void LinkDebugPortals()
    {
        EntityUid? debugPortal = null;
        EntityUid? planetPortal = null;

        var query = EntityQueryEnumerator<MetaDataComponent, LinkedEntityComponent>();

        while (query.MoveNext(out var uid, out var meta, out var linked))
        {
            if (meta.EntityPrototype == null)
                continue;

            var protoId = meta.EntityPrototype.ID;

            if (protoId == DebugToPlanetPortalProto)
                debugPortal = uid;
            else if (protoId == PlanetToDebugPortalProto)
                planetPortal = uid;
        }

        if (debugPortal == null)
        {
            Log.Error("[CoreDebugPlanet] Failed to find PortalGateDebugToPlanet.");
            return;
        }

        if (planetPortal == null)
        {
            Log.Error("[CoreDebugPlanet] Failed to find PortalGatePlanetToDebug.");
            return;
        }

        _linked.OneWayLink(debugPortal.Value, planetPortal.Value);
        _linked.OneWayLink(planetPortal.Value, debugPortal.Value);

        var debugPortalComp = EnsureComp<PortalComponent>(debugPortal.Value);
        var planetPortalComp = EnsureComp<PortalComponent>(planetPortal.Value);

        debugPortalComp.CanTeleportToOtherMaps = true;
        planetPortalComp.CanTeleportToOtherMaps = true;

        debugPortalComp.RandomTeleport = false;
        planetPortalComp.RandomTeleport = false;

        Dirty(debugPortal.Value, debugPortalComp);
        Dirty(planetPortal.Value, planetPortalComp);

        Log.Info($"[CoreDebugPlanet] Linked debug portals: {ToPrettyString(debugPortal.Value)} <-> {ToPrettyString(planetPortal.Value)}");
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        if (_loaded)
            return;

        if (!_mapLoader.TryLoadMap(PlanetMapPath, out var mapEntity, out var grids))
        {
            Log.Error($"[CoreDebugPlanet] Failed to load debug planet map: {PlanetMapPath}");
            return;
        }

        if (mapEntity == null)
        {
            Log.Error($"[CoreDebugPlanet] Loaded map file, but map entity is null: {PlanetMapPath}");
            return;
        }

        var map = mapEntity.Value;
        var mapId = map.Comp.MapId;

        var nullableMap = (map.Owner, (MapComponent?) map.Comp);

        _map.InitializeMap(nullableMap);

        _map.SetPaused(nullableMap, false);

        LinkDebugPortals();

        _loaded = true;

        Log.Info($"[CoreDebugPlanet] Loaded, initialized and unpaused debug planet map {PlanetMapPath} as map {mapId}. Grids: {grids?.Count ?? 0}");
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _loaded = false;
    }
}

#endif
