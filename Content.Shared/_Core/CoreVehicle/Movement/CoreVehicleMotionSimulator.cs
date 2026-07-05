using System;
using System.Numerics;

namespace Content.Shared._Core.CoreVehicle;

/// <summary>
/// Чистый симулятор автомобильного движения.
/// Не знает ничего про EntityUid, Transform, Physics и Buckle.
/// </summary>
public static class CoreVehicleMotionSimulator
{
    private const float StopEpsilon = 0.03f;

    public static CoreVehicleMotionState Simulate(
        CoreVehicleMotionState state,
        CoreVehicleMotionConfig config,
        CoreVehicleInput rawInput,
        float frameTime)
    {
        var input = rawInput.Clamped();

        // Защита от больших лаговых тиков.
        var dt = Math.Clamp(frameTime, 0f, 0.1f);

        var forward = GetForward(state.Rotation);
        var right = GetRight(state.Rotation);

        var forwardSpeed = Vector2.Dot(state.Velocity, forward);
        var sideSpeed = Vector2.Dot(state.Velocity, right);

        StepSteering(ref state, input, config, dt);

        StepForwardSpeed(ref forwardSpeed, input, config, dt);
        StepRotation(ref state, forwardSpeed, input, config, dt);

        // После поворота корпуса пересчитываем оси.
        forward = GetForward(state.Rotation);
        right = GetRight(state.Rotation);

        // После изменения угла прежняя мировая скорость создаёт боковое скольжение.
        forwardSpeed = Vector2.Dot(state.Velocity, forward);
        sideSpeed = Vector2.Dot(state.Velocity, right);

        StepSideGrip(ref sideSpeed, forwardSpeed, state.CurrentSteer, input, config, dt);
        StepTurnSpeedLoss(ref forwardSpeed, state.CurrentSteer, input, config, dt);

        state.Velocity = forward * forwardSpeed + right * sideSpeed;
        state.Position += state.Velocity * dt;

        state.DriftAmount = CalculateDriftAmount(forwardSpeed, sideSpeed, input.Handbrake);

        if (state.Velocity.LengthSquared() < StopEpsilon * StopEpsilon)
            state.Velocity = Vector2.Zero;

        return state;
    }

    private static void StepSteering(
        ref CoreVehicleMotionState state,
        CoreVehicleInput input,
        CoreVehicleMotionConfig config,
        float dt)
    {
        var targetSteer = input.Steer;

        var speed = Math.Abs(targetSteer) > Math.Abs(state.CurrentSteer)
            ? config.SteerResponse
            : config.SteerReturnSpeed;

        state.CurrentSteer = StepTowards(state.CurrentSteer, targetSteer, speed, dt);

        if (Math.Abs(state.CurrentSteer) < 0.001f)
            state.CurrentSteer = 0f;
    }

    private static void StepForwardSpeed(
        ref float forwardSpeed,
        CoreVehicleInput input,
        CoreVehicleMotionConfig config,
        float dt)
    {
        if (input.Handbrake)
        {
            forwardSpeed = StepTowards(forwardSpeed, 0f, config.HandbrakeForce, dt);
            return;
        }

        if (input.Throttle > 0f)
        {
            var target = config.MaxForwardSpeed * input.Throttle;

            if (forwardSpeed < 0f)
                forwardSpeed = StepTowards(forwardSpeed, 0f, config.BrakeForce, dt);
            else
                forwardSpeed = StepTowards(forwardSpeed, target, config.Acceleration, dt);

            return;
        }

        if (input.BrakeReverse > 0f)
        {
            if (forwardSpeed > 0.25f)
            {
                forwardSpeed = StepTowards(forwardSpeed, 0f, config.BrakeForce * input.BrakeReverse, dt);
            }
            else
            {
                var target = -config.MaxReverseSpeed * input.BrakeReverse;
                forwardSpeed = StepTowards(forwardSpeed, target, config.ReverseAcceleration, dt);
            }

            return;
        }

        forwardSpeed = StepTowards(forwardSpeed, 0f, config.RollingResistance, dt);
    }

    private static void StepRotation(
        ref CoreVehicleMotionState state,
        float forwardSpeed,
        CoreVehicleInput input,
        CoreVehicleMotionConfig config,
        float dt)
    {
        var absSpeed = Math.Abs(forwardSpeed);

        if (absSpeed < config.MinSpeedForSteer)
        {
            state.AngularVelocity = StepTowards(state.AngularVelocity, 0f, config.AngularDamping, dt);
            state.Rotation = NormalizeRadians(state.Rotation + state.AngularVelocity * dt);
            return;
        }

        var speedFactor = Math.Clamp(absSpeed / config.SpeedForFullSteer, 0f, 1f);
        var directionSign = Math.Sign(forwardSpeed);

        var turnMultiplier = input.Handbrake
            ? config.HandbrakeTurnMultiplier
            : 1f;

        var targetAngularVelocity =
            state.CurrentSteer *
            config.SteerPower *
            speedFactor *
            directionSign *
            turnMultiplier;

        state.AngularVelocity = StepTowards(
            state.AngularVelocity,
            targetAngularVelocity,
            config.AngularResponse,
            dt);

        state.AngularVelocity = StepTowards(
            state.AngularVelocity,
            0f,
            config.AngularDamping * dt,
            dt);

        state.Rotation = NormalizeRadians(state.Rotation + state.AngularVelocity * dt);
    }

    private static void StepSideGrip(
        ref float sideSpeed,
        float forwardSpeed,
        float steer,
        CoreVehicleInput input,
        CoreVehicleMotionConfig config,
        float dt)
    {
        if (input.Handbrake && Math.Abs(forwardSpeed) > 1f)
        {
            sideSpeed += steer * forwardSpeed * config.HandbrakeDriftForce * dt;
        }

        var grip = input.Handbrake
            ? config.DriftGrip
            : config.Grip;

        sideSpeed = StepTowards(sideSpeed, 0f, grip, dt);
    }

    private static void StepTurnSpeedLoss(
        ref float forwardSpeed,
        float steer,
        CoreVehicleInput input,
        CoreVehicleMotionConfig config,
        float dt)
    {
        var steerAmount = Math.Abs(steer);

        if (steerAmount <= 0.001f)
            return;

        var loss = steerAmount * Math.Abs(forwardSpeed) * config.TurnSpeedLoss;

        if (input.Handbrake)
            loss *= 1.5f;

        forwardSpeed = StepTowards(forwardSpeed, 0f, loss, dt);
    }

    private static float CalculateDriftAmount(float forwardSpeed, float sideSpeed, bool handbrake)
    {
        var total = Math.Abs(forwardSpeed) + Math.Abs(sideSpeed);

        if (total <= 0.001f)
            return 0f;

        var drift = Math.Abs(sideSpeed) / total;

        if (handbrake)
            drift += 0.25f;

        return Math.Clamp(drift, 0f, 1f);
    }

    private static Vector2 GetForward(float radians)
    {
        return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
    }

    private static Vector2 GetRight(float radians)
    {
        return new Vector2(-MathF.Sin(radians), MathF.Cos(radians));
    }

    private static float StepTowards(float current, float target, float speed, float dt)
    {
        var step = Math.Abs(speed) * dt;

        if (current < target)
            return Math.Min(current + step, target);

        if (current > target)
            return Math.Max(current - step, target);

        return current;
    }

    private static float NormalizeRadians(float radians)
    {
        while (radians > MathF.PI)
            radians -= MathF.Tau;

        while (radians < -MathF.PI)
            radians += MathF.Tau;

        return radians;
    }
}
