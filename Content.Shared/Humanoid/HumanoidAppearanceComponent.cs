using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid;

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState(true)]
public sealed partial class HumanoidAppearanceComponent : Component
{
    [DataField]
    public Dictionary<HumanoidVisualLayers, HumanoidSpeciesSpriteLayer> BaseLayers = new();

    [DataField, AutoNetworkedField]
    public HashSet<HumanoidVisualLayers> PermanentlyHidden = new();

    [DataField, AutoNetworkedField]
    public Gender Gender;

    [DataField, AutoNetworkedField]
    public int Age = 18;

    [DataField, AutoNetworkedField]
    public Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> CustomBaseLayers = new();

    [DataField(required: true), AutoNetworkedField]
    public ProtoId<SpeciesPrototype> Species { get; set; }

    [DataField]
    public ProtoId<HumanoidProfilePrototype>? Initial { get; private set; }

    [DataField, AutoNetworkedField]
    public Color SkinColor { get; set; } = Color.FromHex("#C0967F");

    [DataField, AutoNetworkedField]
    public Dictionary<HumanoidVisualLayers, SlotFlags> HiddenLayers = new();

    [DataField, AutoNetworkedField]
    public Sex Sex = Sex.Male;

    [DataField, AutoNetworkedField]
    public Color EyeColor = Color.Brown;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public float Width = 1f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public float Height = 1f;

    [ViewVariables(VVAccess.ReadOnly)]
    public Color? CachedHairColor;

    [ViewVariables(VVAccess.ReadOnly)]
    public Color? CachedFacialHairColor;

    [DataField]
    public HashSet<HumanoidVisualLayers> HideLayersOnEquip = [HumanoidVisualLayers.Hair];
}

[DataDefinition]
[Serializable, NetSerializable]
public readonly partial struct CustomBaseLayerInfo
{
    public CustomBaseLayerInfo(string? id, Color? color = null)
    {
        DebugTools.Assert(id == null || IoCManager.Resolve<IPrototypeManager>().HasIndex<HumanoidSpeciesSpriteLayer>(id));
        Id = id;
        Color = color;
    }

    [DataField]
    public ProtoId<HumanoidSpeciesSpriteLayer>? Id { get; init; }

    [DataField]
    public Color? Color { get; init; }
}
