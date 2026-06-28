using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Prototypes;

/// <summary>
///     Humanoid species sprite layer. This is what defines the base layer of
///     a humanoid species sprite, and also defines how markings can appear over
///     that sprite.
/// </summary>
[Prototype("humanoidBaseSprite")]
public sealed partial class HumanoidSpeciesSpriteLayer : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The base sprite for this sprite layer. This replaces the empty layer
    ///     tagged by the enum tied to this layer.
    /// </summary>
    [DataField("baseSprite")]
    public SpriteSpecifier? BaseSprite { get; private set; }

    [DataField("layerAlpha")]
    public float LayerAlpha { get; private set; } = 1.0f;

    [DataField("allowsMarkings")]
    public bool AllowsMarkings { get; private set; } = true;

    [DataField("matchSkin")]
    public bool MatchSkin { get; private set; } = true;

    [DataField("markingsMatchSkin")]
    public bool MarkingsMatchSkin { get; private set; }
}
