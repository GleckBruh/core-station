using System.Linq;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Shared._Core.StationManager.CoreConfig;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Core.StationManager.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed partial class ForceStationCommand : LocalizedCommands
    {
        [Dependency] private IEntitySystemManager _entitySystemManager = default!;
        [Dependency] private IPrototypeManager _prototypeManager = default!;
        [Dependency] private IEntityManager _entity = default!;
        public override string Command => "forcestation";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteLine(Loc.GetString("shell-need-exactly-one-argument"));
                return;
            }

            var station = args[0];

            if (_prototypeManager.TryIndex<CoreStationConfigPrototype>(station, out var config))
            {
                var coreStation = _entitySystemManager.GetEntitySystem<CoreStationSystem>();
                coreStation.SetActiveConfig(station);
                shell.WriteLine($"Station '{station}' has been loaded");
            }
            else
            {
                shell.WriteLine($"Station config '{station}' not found!");
            }

        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var options = _prototypeManager
                    .EnumeratePrototypes<CoreStationConfigPrototype>()
                    .Select(p => new CompletionOption(p.ID, p.Name.ToString()))
                    .OrderBy(p => p.Value);

                return CompletionResult.FromHintOptions(options, "hint text");
            }
            return CompletionResult.Empty;
        }
    }
}
