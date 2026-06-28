using System.Linq;
using Content.Shared._Starlight.Medical.Limbs;
using Content.Shared.Body;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Starlight;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Starlight.Medical.Limbs;

public sealed partial class LimbSystem : SharedLimbSystem
{
    public void AddLimbVisual(Entity<HumanoidAppearanceComponent> body, Entity<BodyPartComponent> limb)
    {
        var layers = new List<HumanoidVisualLayers>();

        foreach (var partLimbId in _body.GetBodyPartAdjacentParts(limb.Owner, limb.Comp).Concat([limb.Owner]))
        {
            if (!TryComp(partLimbId, out BodyPartComponent? partLimb))
                continue;

            var layer = ToHumanoidLayer(partLimb);
            if (layer is null)
                continue;

            layers.Add(layer.Value);

            if (TryComp<BaseLayerIdComponent>(partLimbId, out var baseLayerStorage)
                && baseLayerStorage.Layer is { } layerId)
            {
                SetBaseLayerId(body, layer.Value, layerId);

                if (_prototype.TryIndex(layerId, out HumanoidSpeciesSpriteLayer? baseLayer))
                    SetBaseLayerColor(body, layer.Value, baseLayer.MatchSkin ? body.Comp.SkinColor : Color.White);
            }
        }

        SetLayersVisibility(body, layers, true);
    }

    private void RemoveLimbVisual(
        Entity<TransformComponent, HumanoidAppearanceComponent, BodyComponent> body,
        Entity<TransformComponent, MetaDataComponent, BodyPartComponent> limb)
    {
        var humanoid = body.Comp2;
        var layers = new List<HumanoidVisualLayers>();

        foreach (var partLimbId in _body.GetBodyPartAdjacentParts(limb.Owner, limb.Comp3).Concat([limb.Owner]))
        {
            if (!TryComp(partLimbId, out BodyPartComponent? partLimb))
                continue;

            var layer = ToHumanoidLayer(partLimb);
            if (layer is null)
                continue;

            layers.Add(layer.Value);

            // Save the current custom base layer on the amputated part,
            // so a later reattach can restore it.
            var baseLayerStorage = EnsureComp<BaseLayerIdComponent>(partLimbId);

            if (humanoid.CustomBaseLayers.TryGetValue(layer.Value, out var customBaseLayer))
                baseLayerStorage.Layer = customBaseLayer.Id;
            else
                baseLayerStorage.Layer = null;

            Dirty(partLimbId, baseLayerStorage);
        }

        SetLayersVisibility((body.Owner, body.Comp2), layers, false);
    }

    public void ToggleLimbVisual(
        Entity<HumanoidAppearanceComponent> body,
        Entity<BaseLayerIdComponent, BaseLayerIdToggledComponent, BodyPartComponent> limb,
        bool toggled)
    {
        var layer = ToHumanoidLayer(limb.Comp3);
        if (layer is null)
            return;

        SetBaseLayerId(body, layer.Value, toggled ? limb.Comp2.Layer : limb.Comp1.Layer);
    }

    private static HumanoidVisualLayers? ToHumanoidLayer(BodyPartComponent part)
    {
        return part.PartType switch
        {
            BodyPartType.Arm => part.Symmetry == BodyPartSymmetry.Right
                ? HumanoidVisualLayers.RArm
                : HumanoidVisualLayers.LArm,

            BodyPartType.Hand => part.Symmetry == BodyPartSymmetry.Right
                ? HumanoidVisualLayers.RHand
                : HumanoidVisualLayers.LHand,

            BodyPartType.Leg => part.Symmetry == BodyPartSymmetry.Right
                ? HumanoidVisualLayers.RLeg
                : HumanoidVisualLayers.LLeg,

            BodyPartType.Foot => part.Symmetry == BodyPartSymmetry.Right
                ? HumanoidVisualLayers.RFoot
                : HumanoidVisualLayers.LFoot,

            BodyPartType.Tail => HumanoidVisualLayers.Tail,

            _ => null,
        };
    }

    private void SetLayersVisibility(
        Entity<HumanoidAppearanceComponent> body,
        IEnumerable<HumanoidVisualLayers> layers,
        bool visible)
    {
        foreach (var layer in layers.Distinct())
        {
            if (visible)
                body.Comp.PermanentlyHidden.Remove(layer);
            else
                body.Comp.PermanentlyHidden.Add(layer);
        }

        Dirty(body, body.Comp);
    }

    private void SetBaseLayerId(
        Entity<HumanoidAppearanceComponent> body,
        HumanoidVisualLayers layer,
        ProtoId<HumanoidSpeciesSpriteLayer>? layerId)
    {
        if (layerId is null)
            body.Comp.CustomBaseLayers.Remove(layer);
        else if (body.Comp.CustomBaseLayers.TryGetValue(layer, out var oldInfo))
            body.Comp.CustomBaseLayers[layer] = new CustomBaseLayerInfo(layerId, oldInfo.Color);
        else
            body.Comp.CustomBaseLayers[layer] = new CustomBaseLayerInfo(layerId);

        Dirty(body, body.Comp);
    }

    private void SetBaseLayerColor(
        Entity<HumanoidAppearanceComponent> body,
        HumanoidVisualLayers layer,
        Color color)
    {
        if (body.Comp.CustomBaseLayers.TryGetValue(layer, out var oldInfo))
            body.Comp.CustomBaseLayers[layer] = new CustomBaseLayerInfo(oldInfo.Id, color);
        else
            body.Comp.CustomBaseLayers[layer] = new CustomBaseLayerInfo(null, color);

        Dirty(body, body.Comp);
    }
}
