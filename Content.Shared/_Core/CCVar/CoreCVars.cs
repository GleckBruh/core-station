using Robust.Shared.Configuration;

namespace Content.Shared._Core.CCVar;

public sealed partial class CoreCVars
{
    public static readonly CVarDef<bool> DebugPlanetEnabled =
        CVarDef.Create("core.debug_planet.enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<string> DebugPlanetPath =
        CVarDef.Create("core.debug_planet.path", "/Maps/_Core/Debug/core_debug_planet.yml", CVar.SERVERONLY);
}
