using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Core.CoreVehicle;

/// <summary>
/// Текущее физическое состояние машины.
/// Это чистая математика, без EntityUid и SS14 API.
/// </summary>
[Serializable, NetSerializable]
public record struct CoreVehicleMotionState
{
    /// <summary>
    /// Локальная позиция машины.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Текущая скорость машины в world/local координатах.
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// Поворот корпуса в радианах.
    /// 0 радиан = машина смотрит вправо по X.
    /// </summary>
    public float Rotation;

    /// <summary>
    /// Угловая скорость в радианах в секунду.
    /// </summary>
    public float AngularVelocity;

    /// <summary>
    /// Текущее положение руля от -1 до 1.
    /// Не прыгает мгновенно, а плавно догоняет ввод.
    /// </summary>
    public float CurrentSteer;

    /// <summary>
    /// Насколько машина сейчас скользит боком.
    /// 0 = держится дороги.
    /// 1 = сильный занос.
    /// </summary>
    public float DriftAmount;
}
