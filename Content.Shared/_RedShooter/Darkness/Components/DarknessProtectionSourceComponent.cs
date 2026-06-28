namespace Content.Shared._RedShooter.Darkness.Components;

/// <summary>
/// Источник световой защиты от Мрака.
/// Например фонарь, лампа, прожектор, аварийный светильник.
/// </summary>
[RegisterComponent]
public sealed partial class DarknessProtectionSourceComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Radius = 5f;

    /// <summary>
    /// По умолчанию источник должен иметь активный PointLight.
    /// Для ExpendableLight используется отдельная проверка состояния.
    /// </summary>
    [DataField]
    public bool RequiresActiveLight = true;

    /// <summary>
    /// Если true, у ExpendableLight во время Fading радиус защиты уменьшается.
    /// </summary>
    [DataField]
    public bool ScaleWithExpendableLightFade = true;

    /// <summary>
    /// Ниже этой доли затухания ExpendableLight уже не защищает.
    /// 0.1 = последние 10% fade-времени защиты уже нет.
    /// </summary>
    [DataField]
    public float MinimumExpendableFadeFraction = 0.1f;

    [DataField]
    public float MinimumProtectionRadius = 0.5f;
}
