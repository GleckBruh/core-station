using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Starlight;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Medical.Limbs;
public abstract class SharedLimbSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WithAttachedBodyPartsComponent, MapInitEvent>(OnWithAttachedBodyPartsMapInit);

    }

    private void OnWithAttachedBodyPartsMapInit(Entity<WithAttachedBodyPartsComponent> ent, ref MapInitEvent args)
    {
        foreach (var partProtoId in ent.Comp.Parts)
        {
            if (!_prototypes.TryIndex(partProtoId.Value, out var prototype))
                continue;

            var slotId = GetPartSlotContainerId(partProtoId.Key);

            _containers.EnsureContainer<ContainerSlot>(ent, slotId);
            _ = SpawnInContainerOrDrop(prototype.ID, ent, slotId);
        }
    }

    private static string GetPartSlotContainerId(string slot)
    {
        return $"body_part_{slot}";
    }
}
