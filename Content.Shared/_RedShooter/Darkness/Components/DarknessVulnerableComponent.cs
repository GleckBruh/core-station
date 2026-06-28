using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._RedShooter.Darkness.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DarknessVulnerableComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Exposure = 0f;

    [DataField, AutoNetworkedField]
    public float MaxExposure = 100f;

    [DataField]
    public float ExposureGainPerSecond = 8f;

    [DataField]
    public float ExposureDecayPerSecond = 12f;

    [DataField]
    public float LightDamageThreshold = 20f;

    [DataField]
    public float MediumDamageThreshold = 50f;

    [DataField]
    public float HeavyDamageThreshold = 80f;

    [DataField]
    public TimeSpan LightDamageInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan MediumDamageInterval = TimeSpan.FromSeconds(1.5);

    [DataField]
    public TimeSpan HeavyDamageInterval = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextDamageTime = TimeSpan.Zero;

    [DataField]
    public DamageSpecifier LightDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", 3 }
        }
    };

    [DataField]
    public DamageSpecifier MediumDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", 6 }
        }
    };

    [DataField]
    public DamageSpecifier HeavyDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", 10 }
        }
    };

    [DataField]
    public SoundSpecifier DamageSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/energy_meat1.ogg");

    [DataField]
    public float WarningExposureFraction = 0.15f;

    [DataField]
    public TimeSpan WarningInterval = TimeSpan.FromSeconds(14);

    [DataField]
    public TimeSpan NextWarningTime = TimeSpan.Zero;
}
