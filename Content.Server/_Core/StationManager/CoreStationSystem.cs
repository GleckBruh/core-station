using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Maps;
using Content.Shared._Core.StationManager.CoreConfig;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Core.StationManager
{
    public sealed partial class CoreStationSystem : EntitySystem
    {
        [Dependency] private IPrototypeManager _prototypeManager = default!;
        [Dependency] private IGameMapManager _mapManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        }

        public void SetActiveConfig(string configId)
        {
            _activeConfigId = configId;
            if (_prototypeManager.TryIndex<CoreStationConfigPrototype>(configId, out var config))
            {
                _mapManager.SelectMap(config.Map);
                Log.Info($"CoreStation: Map '{config.Map}' selected for next round");

                var ticker = EntityManager.System<GameTicker>();
                if (ticker.TryFindGamePreset(config.GamePreset, out var preset))
                {
                    ticker.SetGamePreset(preset, false);
                    Log.Info($"CoreStation: GamePreset '{config.GamePreset}' selected for next round");
                }
                else
                {
                    Log.Warning($"CoreStation: GamePreset '{config.GamePreset}' not found! Using default.");
                }
            }
            else
            {
                Log.Error($"CoreStation: Config prototype '{_activeConfigId}' not found!");
            }
        }

        private string? _activeConfigId;

        private void OnRoundStarting(RoundStartingEvent args)
        {
            if (_activeConfigId == null)
            {
                Log.Warning("CoreStation: No active config set! Round starting without station");
                return;
            }
        }
    }
}
