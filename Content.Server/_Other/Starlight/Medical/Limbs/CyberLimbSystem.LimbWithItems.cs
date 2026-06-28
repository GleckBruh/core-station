using System.Linq;
using Content.Shared._Starlight.Medical.Limbs;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Starlight;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using static Content.Server.Power.Pow3r.PowerState;

namespace Content.Server._Starlight.Medical.Limbs;

public sealed partial class CyberLimbSystem : EntitySystem
{
    public void InitializeLimbWithItems()
    {
        base.Initialize();
        SubscribeLocalEvent<LimbWithItemsComponent, ToggleLimbEvent>(OnLimbToggle);
        SubscribeLocalEvent<BodyComponent, LimbRemovedEvent<LimbWithItemsComponent>>(LimbWithItemsRemoved);

    }

    private void LimbWithItemsRemoved(Entity<BodyComponent> ent, ref LimbRemovedEvent<LimbWithItemsComponent> args)
    {
        if (args.Comp.Toggled)
        {
            var toggleLimbEvent = new ToggleLimbEvent()
            {
                Performer = ent.Owner,
            };
            OnLimbToggle((args.Limb, args.Comp), ref toggleLimbEvent);
        }
    }

    private void OnLimbToggle(Entity<LimbWithItemsComponent> ent, ref ToggleLimbEvent args)
    {
        if (!TryComp<LimbItemStorageComponent>(ent, out var storage))
            return;

        ent.Comp.Toggled = !ent.Comp.Toggled;

        if (ent.Comp.Toggled)
        {
            foreach (var item in storage.ItemEntities)
            {
                var handId = $"{ent.Owner}_{item}";
                var hands = EnsureComp<HandsComponent>(args.Performer);
                _hands.AddHand((args.Performer, hands), handId, HandLocation.Middle);
                _hands.DoPickup(args.Performer, handId, item, hands);
                EnsureComp<UnremoveableComponent>(item);
            }
        }
        else
        {
            var container = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId, out _);
            foreach (var item in storage.ItemEntities)
            {
                var xform = Transform(item);
                var meta = MetaData(item);
                var physics = EnsureComp<PhysicsComponent>(item);
                var handId = $"{ent.Owner}_{item}";
                RemComp<UnremoveableComponent>(item);
                _container.Insert((item, xform, meta, physics), container, force: true);
                _hands.RemoveHand(args.Performer, handId);
            }
        }

        if (TryComp<BaseLayerIdComponent>(ent.Owner, out var baseLayer)
            && TryComp<BaseLayerIdToggledComponent>(ent.Owner, out var toggledLayer)
            && TryComp<BodyPartComponent>(ent.Owner, out var bodyPart)
            && TryComp<HumanoidAppearanceComponent>(args.Performer, out var humanoid))
        {
            _limb.ToggleLimbVisual(
                (args.Performer, humanoid),
                (ent.Owner, baseLayer, toggledLayer, bodyPart),
                ent.Comp.Toggled);
        }

        _audio.PlayPvs(ent.Comp.Sound, args.Performer);

        Dirty(ent);
    }
}
