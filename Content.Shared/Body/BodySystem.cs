using Content.Shared.DragDrop;
using Robust.Shared.Containers;

namespace Content.Shared.Body;

/// <summary>
/// System responsible for coordinating entities with <see cref="BodyComponent" /> and their entities with <see cref="OrganComponent" />.
/// This system is primarily responsible for event relaying and the relationships between a body and its organs.
/// It is not responsible for player-facing body features, e.g. "blood" or "breathing."
/// Such features should be implemented in systems relying on the various events raised by this class.
/// </summary>
/// <seealso cref="OrganGotInsertedEvent" />
/// <seealso cref="OrganGotRemovedEvent" />
/// <seealso cref="OrganInsertedIntoEvent" />
/// <seealso cref="OrganRemovedFromEvent" />
/// <seealso cref="BodyRelayedEvent{TEvent}" />
public sealed partial class BodySystem : EntitySystem
{
    /// <summary>
    /// Container ID prefix for child body-part slots.
    /// Imported for Sunrise/Starlight surgery compatibility.
    /// </summary>
    public const string PartSlotContainerIdPrefix = "body_part_slot_";

    /// <summary>
    /// Compatibility root container id used by Sunrise body-part code.
    /// Your current body system still uses <see cref="BodyComponent.ContainerID"/> for normal organs.
    /// </summary>
    public const string BodyRootContainerId = "body_root_part";

    /// <summary>
    /// Container ID prefix for organ slots inside body parts.
    /// Imported for Sunrise/Starlight surgery compatibility.
    /// </summary>
    public const string OrganSlotContainerIdPrefix = "body_organ_slot_";

    [Dependency] private SharedContainerSystem _container = default!;

    [Dependency] private EntityQuery<BodyComponent> _bodyQuery = default!;
    [Dependency] private EntityQuery<OrganComponent> _organQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, ComponentInit>(OnBodyInit);
        SubscribeLocalEvent<BodyComponent, ComponentShutdown>(OnBodyShutdown);

        SubscribeLocalEvent<BodyComponent, CanDragEvent>(OnCanDrag);

        SubscribeLocalEvent<BodyComponent, EntInsertedIntoContainerMessage>(OnBodyEntInserted);
        SubscribeLocalEvent<BodyComponent, EntRemovedFromContainerMessage>(OnBodyEntRemoved);

        InitializeParts();
        InitializeRelay();
    }

    /// <summary>
    /// Inverse of <see cref="GetPartSlotContainerId"/>.
    /// </summary>
    protected static string? GetPartSlotContainerIdFromContainer(string containerSlotId)
    {
        var slotIndex = containerSlotId.IndexOf(PartSlotContainerIdPrefix, StringComparison.Ordinal);

        if (slotIndex < 0)
            return null;

        return containerSlotId.Remove(slotIndex, PartSlotContainerIdPrefix.Length);
    }

    /// <summary>
    /// Gets the container id for a child body-part slot.
    /// </summary>
    public static string GetPartSlotContainerId(string slotId)
    {
        return PartSlotContainerIdPrefix + slotId;
    }

    /// <summary>
    /// Gets the container id for an organ slot inside a body part.
    /// </summary>
    public static string GetOrganContainerId(string slotId)
    {
        return OrganSlotContainerIdPrefix + slotId;
    }

    private void OnBodyInit(Entity<BodyComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Organs =
            _container.EnsureContainer<Container>(ent, BodyComponent.ContainerID);
    }

    private void OnBodyShutdown(Entity<BodyComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Organs is { } organs)
            _container.ShutdownContainer(organs);
    }

    private void OnBodyEntInserted(Entity<BodyComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != BodyComponent.ContainerID)
            return;

        if (!_organQuery.TryComp(args.Entity, out var organ))
            return;

        var body = new OrganInsertedIntoEvent(args.Entity);
        RaiseLocalEvent(ent, ref body);

        var ev = new OrganGotInsertedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        if (organ.Body != ent)
        {
            organ.Body = ent;
            Dirty(args.Entity, organ);
        }
    }

    private void OnBodyEntRemoved(Entity<BodyComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != BodyComponent.ContainerID)
            return;

        if (!_organQuery.TryComp(args.Entity, out var organ))
            return;

        var body = new OrganRemovedFromEvent(args.Entity);
        RaiseLocalEvent(ent, ref body);

        var ev = new OrganGotRemovedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        if (organ.Body == null)
            return;

        organ.Body = null;
        Dirty(args.Entity, organ);
    }

    private void OnCanDrag(Entity<BodyComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }
}
