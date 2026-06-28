namespace Content.Shared._RedShooter.Darkness.Components;

/// <summary>
/// Предмет экипировки, защищающий носителя от Мрака.
/// Например, герметичный скафандр.
/// </summary>
[RegisterComponent]
public sealed partial class DarknessProtectionEquipmentComponent : Component
{
    [DataField]
    public bool Enabled = true;
}
