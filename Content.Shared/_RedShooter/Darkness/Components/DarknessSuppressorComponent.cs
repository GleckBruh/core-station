namespace Content.Shared._RedShooter.Darkness.Components;

/// <summary>
/// Убирает действие Мрака в радиусе.
/// Используется для городков / зон, где Мрака нет.
/// </summary>
[RegisterComponent]
public sealed partial class DarknessSuppressorComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Radius = 10f;
}
