using System.Linq;
using Content.Server.Actions;
using Content.Server.Hands.Systems;
using Content.Shared.Starlight;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Starlight.Medical.Limbs;

public sealed partial class LimbItemStorageSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LimbItemStorageComponent, MapInitEvent>(OnLimbItemStorageInit);
    }

    private void OnLimbItemStorageInit(Entity<LimbItemStorageComponent> limb, ref MapInitEvent args)
    {
        if (limb.Comp.ItemEntities?.Count == limb.Comp.Items.Count) return;
        var container = _container.EnsureContainer<Container>(limb.Owner, limb.Comp.ContainerId, out _);

        limb.Comp.ItemEntities = [.. limb.Comp.Items.Select(EnsureItem)];

        Dirty(limb);

        EntityUid EnsureItem(EntProtoId proto)
        {
            var id = Spawn(proto);

            var xform = Transform(id);
            var meta = MetaData(id);
            var physics = EnsureComp<PhysicsComponent>(id);

            _container.Insert((id, xform, meta, physics), container, force: true);

            return id;
        }
    }
}
