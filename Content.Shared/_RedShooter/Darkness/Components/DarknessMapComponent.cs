using Robust.Shared.GameStates;

namespace Content.Shared._RedShooter.Darkness.Components;

/// <summary>
/// Если этот компонент висит на grid, весь grid по умолчанию считается покрытым Мраком.
/// </summary>
[RegisterComponent]
public sealed partial class DarknessMapComponent : Component
{
    [DataField]
    public bool Enabled = true;
}
