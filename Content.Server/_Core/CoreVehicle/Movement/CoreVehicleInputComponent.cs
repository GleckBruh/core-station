using Robust.Shared.GameStates;

namespace Content.Shared._Core.CoreVehicle;

/// <summary>
/// Временное хранилище ввода машины.
/// Позже сюда будет писать система водительского места.
/// </summary>
[RegisterComponent]
public sealed partial class CoreVehicleInputComponent : Component
{
    /// <summary>
    /// Текущий ввод машины.
    /// </summary>
    [AutoNetworkedField]
    public CoreVehicleInput CurrentInput = CoreVehicleInput.Empty;
}
