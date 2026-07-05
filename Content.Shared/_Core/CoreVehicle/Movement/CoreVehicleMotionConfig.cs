using Robust.Shared.Serialization;

namespace Content.Shared._Core.CoreVehicle;

/// <summary>
/// Настройки физики конкретной машины.
/// Эти значения потом можно будет вынести в YAML через компонент.
/// </summary>
[Serializable, NetSerializable]
public record struct CoreVehicleMotionConfig
{
    public float MaxForwardSpeed;
    public float MaxReverseSpeed;

    public float Acceleration;
    public float ReverseAcceleration;

    public float BrakeForce;
    public float HandbrakeForce;

    public float RollingResistance;

    public float SteerResponse;
    public float SteerReturnSpeed;

    public float SteerPower;
    public float AngularResponse;
    public float AngularDamping;

    public float Grip;
    public float DriftGrip;

    public float TurnSpeedLoss;
    public float HandbrakeDriftForce;
    public float HandbrakeTurnMultiplier;

    public float MinSpeedForSteer;
    public float SpeedForFullSteer;

    public static CoreVehicleMotionConfig Default => new()
    {
        MaxForwardSpeed = 9f,
        MaxReverseSpeed = 3f,

        Acceleration = 5f,
        ReverseAcceleration = 3f,

        BrakeForce = 12f,
        HandbrakeForce = 18f,

        RollingResistance = 1.2f,

        SteerResponse = 5f,
        SteerReturnSpeed = 7f,

        SteerPower = 2.8f,
        AngularResponse = 8f,
        AngularDamping = 5f,

        Grip = 10f,
        DriftGrip = 3.5f,

        TurnSpeedLoss = 0.8f,
        HandbrakeDriftForce = 4f,
        HandbrakeTurnMultiplier = 1.35f,

        MinSpeedForSteer = 0.4f,
        SpeedForFullSteer = 5f,
    };
}
