using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Core.CoreVehicle;

[RegisterComponent]
public sealed partial class CoreVehicleMoverComponent : Component
{
    [DataField]
    public float MaxForwardSpeed = 9f;

    [DataField]
    public float MaxReverseSpeed = 3f;

    [DataField]
    public float Acceleration = 5f;

    [DataField]
    public float ReverseAcceleration = 3f;

    [DataField]
    public float BrakeForce = 12f;

    [DataField]
    public float HandbrakeForce = 18f;

    [DataField]
    public float RollingResistance = 1.2f;

    [DataField]
    public float SteerResponse = 5f;

    [DataField]
    public float SteerReturnSpeed = 7f;

    [DataField]
    public float SteerPower = 2.8f;

    [DataField]
    public float AngularResponse = 8f;

    [DataField]
    public float AngularDamping = 5f;

    [DataField]
    public float Grip = 10f;

    [DataField]
    public float DriftGrip = 3.5f;

    [DataField]
    public float TurnSpeedLoss = 0.8f;

    [DataField]
    public float HandbrakeDriftForce = 4f;

    [DataField]
    public float HandbrakeTurnMultiplier = 1.35f;

    [DataField]
    public float MinSpeedForSteer = 0.4f;

    [DataField]
    public float SpeedForFullSteer = 5f;

    public Vector2 Velocity;

    public float AngularVelocity;

    public float CurrentSteer;

    public float DriftAmount;

    public bool HandbrakeActive;

    public bool EngineEnabled = true;

    public CoreVehicleMotionConfig GetConfig()
    {
        return new CoreVehicleMotionConfig
        {
            MaxForwardSpeed = MaxForwardSpeed,
            MaxReverseSpeed = MaxReverseSpeed,

            Acceleration = Acceleration,
            ReverseAcceleration = ReverseAcceleration,

            BrakeForce = BrakeForce,
            HandbrakeForce = HandbrakeForce,

            RollingResistance = RollingResistance,

            SteerResponse = SteerResponse,
            SteerReturnSpeed = SteerReturnSpeed,

            SteerPower = SteerPower,
            AngularResponse = AngularResponse,
            AngularDamping = AngularDamping,

            Grip = Grip,
            DriftGrip = DriftGrip,

            TurnSpeedLoss = TurnSpeedLoss,
            HandbrakeDriftForce = HandbrakeDriftForce,
            HandbrakeTurnMultiplier = HandbrakeTurnMultiplier,

            MinSpeedForSteer = MinSpeedForSteer,
            SpeedForFullSteer = SpeedForFullSteer,
        };
    }
}
