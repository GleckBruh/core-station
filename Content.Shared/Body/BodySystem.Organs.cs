using System.Diagnostics.CodeAnalysis;
using Content.Shared.Body.Part;
using Robust.Shared.Containers;

namespace Content.Shared.Body;

public sealed partial class BodySystem
{
    private void AddOrgan(
        Entity<OrganComponent> organEnt,
        EntityUid bodyUid,
        EntityUid parentPartUid)
    {
        organEnt.Comp.Body = bodyUid;

        var ev = new OrganGotInsertedEvent(bodyUid);
        RaiseLocalEvent(organEnt.Owner, ref ev);

        Dirty(organEnt, organEnt.Comp);
    }

    private void RemoveOrgan(Entity<OrganComponent> organEnt, EntityUid parentPartUid)
    {
        if (organEnt.Comp.Body is { Valid: true } bodyUid)
        {
            var ev = new OrganGotRemovedEvent(bodyUid);
            RaiseLocalEvent(organEnt.Owner, ref ev);
        }

        organEnt.Comp.Body = null;
        Dirty(organEnt, organEnt.Comp);
    }

    private OrganSlot? CreateOrganSlot(Entity<BodyPartComponent?> parentEnt, string slotId)
    {
        if (!Resolve(parentEnt, ref parentEnt.Comp, logMissing: false))
            return null;

        _container.EnsureContainer<ContainerSlot>(parentEnt, GetOrganContainerId(slotId));
        var slot = new OrganSlot(slotId);
        parentEnt.Comp.Organs.Add(slotId, slot);
        Dirty(parentEnt, parentEnt.Comp);
        return slot;
    }

    public bool TryCreateOrganSlot(
        EntityUid? parent,
        string slotId,
        [NotNullWhen(true)] out OrganSlot? slot,
        BodyPartComponent? part = null)
    {
        slot = null;

        if (parent is null || !Resolve(parent.Value, ref part, logMissing: false))
            return false;

        _container.EnsureContainer<ContainerSlot>(parent.Value, GetOrganContainerId(slotId));
        slot = new OrganSlot(slotId);

        if (!part.Organs.TryAdd(slotId, slot.Value))
            return false;

        Dirty(parent.Value, part);
        return true;
    }

    public bool CanInsertOrgan(
        EntityUid partId,
        string slotId,
        BodyPartComponent? part = null)
    {
        return Resolve(partId, ref part) && part.Organs.ContainsKey(slotId);
    }

    public bool CanInsertOrgan(
        EntityUid partId,
        OrganSlot slot,
        BodyPartComponent? part = null)
    {
        return CanInsertOrgan(partId, slot.Id, part);
    }

    public bool InsertOrgan(
        EntityUid partId,
        EntityUid organId,
        string slotId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(organId, ref organ, logMissing: false)
            || !Resolve(partId, ref part, logMissing: false)
            || !CanInsertOrgan(partId, slotId, part))
            return false;

        var containerId = GetOrganContainerId(slotId);

        return _container.TryGetContainer(partId, containerId, out var container)
            && _container.Insert(organId, container);
    }

    public bool RemoveOrgan(EntityUid organId, OrganComponent? organ = null)
    {
        if (!_container.TryGetContainingContainer((organId, null, null), out var container))
            return false;

        var parent = container.Owner;

        return HasComp<BodyPartComponent>(parent)
            && _container.Remove(organId, container);
    }

    public bool AddOrganToFirstValidSlot(
        EntityUid partId,
        EntityUid organId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(partId, ref part, logMissing: false)
            || !Resolve(organId, ref organ, logMissing: false))
            return false;

        foreach (var slotId in part.Organs.Keys)
        {
            InsertOrgan(partId, organId, slotId, part, organ);
            return true;
        }

        return false;
    }

    public List<Entity<T, OrganComponent>> GetBodyOrganEntityComps<T>(
        Entity<BodyComponent?> entity)
        where T : IComponent
    {
        if (!Resolve(entity, ref entity.Comp))
            return new List<Entity<T, OrganComponent>>();

        var query = GetEntityQuery<T>();
        var list = new List<Entity<T, OrganComponent>>(3);

        foreach (var organ in GetBodyOrgans(entity.Owner, entity.Comp))
        {
            if (query.TryGetComponent(organ.Id, out var comp))
                list.Add((organ.Id, comp, organ.Component));
        }

        return list;
    }

    public bool TryGetBodyOrganEntityComps<T>(
        Entity<BodyComponent?> entity,
        [NotNullWhen(true)] out List<Entity<T, OrganComponent>>? comps)
        where T : IComponent
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
        {
            comps = null;
            return false;
        }

        comps = GetBodyOrganEntityComps<T>(entity);

        if (comps.Count != 0)
            return true;

        comps = null;
        return false;
    }

    public IEnumerable<(EntityUid Id, OrganComponent Component)> GetBodyOrgans(
        EntityUid? bodyId,
        BodyComponent? body = null)
    {
        if (!bodyId.HasValue || !Resolve(bodyId.Value, ref body, logMissing: false))
            yield break;

        foreach (var organUid in body.Organs?.ContainedEntities ?? [])
        {
            if (TryComp(organUid, out OrganComponent? organ))
                yield return (organUid, organ);
        }

        foreach (var part in GetBodyChildren(bodyId, body))
        {
            foreach (var organ in GetPartOrgans(part.Id, part.Component))
                yield return organ;
        }
    }
}
