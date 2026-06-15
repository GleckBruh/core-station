using Content.Shared._Core.StationManager.StationOwner;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Shared._Core.StationManager.CoreConfig
{
    [Prototype]
    public sealed partial class CoreStationConfigPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public LocId Name { get; private set; } = "???";

        [DataField]
        public LocId Location { get; private set; } = "???";

        [DataField]
        public LocId Description { get; private set; } = "???";

        [DataField]
        public ProtoId<CoreStationOwnerPrototype> Owner { get; private set; }

        [DataField]
        public string Preview { get; private set; } = "/Textures/CoreStation/Previews/Default.png";

        [DataField(required: true)]
        public ProtoId<GameMapPrototype> Map { get; private set; }

        [DataField]
        public string GamePreset { get; private set; } = "Extended";
    }
}
