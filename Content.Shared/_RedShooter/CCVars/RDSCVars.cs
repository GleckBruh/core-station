using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> DarknessEnabled =
        CVarDef.Create("redshooter.darkness.enabled", true, CVar.SERVERONLY);
}
