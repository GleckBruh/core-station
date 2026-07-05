using Content.Shared._Core.CoreVehicle;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Server._Core.CoreVehicle;

/// <summary>
/// Серверная система движения машин.
/// Берёт состояние из CoreVehicleMoverComponent,
/// прогоняет его через CoreVehicleMotionSimulator
/// и применяет результат к Transform машины.
/// </summary>
public sealed class CoreVehicleMoveSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.Paused)
            return;

        var query = EntityQueryEnumerator<
            CoreVehicleMoverComponent,
            CoreVehicleInputComponent,
            TransformComponent>();

        while (query.MoveNext(out var uid, out var mover, out var input, out var xform))
        {
            UpdateVehicle(uid, mover, input, xform, frameTime);
        }
    }

    private void UpdateVehicle(
        EntityUid uid,
        CoreVehicleMoverComponent mover,
        CoreVehicleInputComponent input,
        TransformComponent xform,
        float frameTime)
    {
        if (!mover.EngineEnabled)
            return;

        // Пока не трогаем машины, которые не находятся на нормальном гриде.
        // Это защитит от странностей при спавне, удалении карты и переносах.
        if (xform.GridUid == null)
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out _))
            return;

        var state = new CoreVehicleMotionState
        {
            Position = xform.LocalPosition,
            Velocity = mover.Velocity,
            Rotation = (float) xform.LocalRotation.Theta,
            AngularVelocity = mover.AngularVelocity,
            CurrentSteer = mover.CurrentSteer,
            DriftAmount = mover.DriftAmount,
        };

        var config = mover.GetConfig();

        state = CoreVehicleMotionSimulator.Simulate(
            state,
            config,
            input.CurrentInput,
            frameTime);

        mover.Velocity = state.Velocity;
        mover.AngularVelocity = state.AngularVelocity;
        mover.CurrentSteer = state.CurrentSteer;
        mover.DriftAmount = state.DriftAmount;
        mover.HandbrakeActive = input.CurrentInput.Handbrake;

        _transform.SetLocalPosition(uid, state.Position, xform);
        _transform.SetLocalRotation(uid, new Angle(state.Rotation), xform);

    }
}
