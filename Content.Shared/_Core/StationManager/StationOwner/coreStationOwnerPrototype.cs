using Robust.Shared.Prototypes;

namespace Content.Shared._Core.StationManager.StationOwner
{
    [Prototype]
    public sealed partial class CoreStationOwnerPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public LocId Name { get; private set; }

        [DataField]
        public string Widget { get; private set; } = default!;
    }
}
