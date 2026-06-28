namespace Content.Shared._RedShooter.Darkness.Components;

/// <summary>
/// Принудительно создаёт Мрак в радиусе.
/// В основном для debug, ивентов или маленьких локальных зон.
/// </summary>
[RegisterComponent]
public sealed partial class DarknessZoneComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Radius = 10f;
}
