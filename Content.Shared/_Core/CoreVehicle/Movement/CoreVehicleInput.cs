using Robust.Shared.Serialization;

namespace Content.Shared._Core.CoreVehicle;

/// <summary>
/// Сырой ввод водителя.
/// Это не направление движения, а именно управление машиной:
/// газ, тормоз/задний ход, руль, ручник.
/// </summary>
public readonly record struct CoreVehicleInput(
    float Throttle,
    float BrakeReverse,
    float Steer,
    bool Handbrake)
{
    public static readonly CoreVehicleInput Empty = new(0f, 0f, 0f, false);

    public CoreVehicleInput Clamped()
    {
        return new CoreVehicleInput(
            Math.Clamp(Throttle, 0f, 1f),
            Math.Clamp(BrakeReverse, 0f, 1f),
            Math.Clamp(Steer, -1f, 1f),
            Handbrake);
    }
}
