using System.Diagnostics.CodeAnalysis;
using Content.Server._Core.AudioDirector.Targets;

namespace Content.Server._Core.AudioDirector;

public sealed class AudioDirectorSystem : EntitySystem
{
    private readonly Dictionary<int, AudioGroup> _groups = new();

    private int _nextGroupId = 1;

    public event Action? StateChanged;

    public IReadOnlyCollection<AudioGroup> Groups =>
        _groups.Values;

    public AudioGroup CreateGroup(
        string name,
        AudioGroupTarget target)
    {
        var id = _nextGroupId;
        _nextGroupId++;

        var group = new AudioGroup(
            id,
            name,
            target);

        _groups.Add(id, group);

        RaiseStateChanged();

        return group;
    }

    public bool TryGetGroup(
        int id,
        [NotNullWhen(true)] out AudioGroup? group)
    {
        return _groups.TryGetValue(id, out group);
    }

    public bool TryRemoveGroup(int id)
    {
        if (!_groups.Remove(id))
            return false;

        RaiseStateChanged();
        return true;
    }

    public bool TryRenameGroup(
        int id,
        string name)
    {
        if (!TryGetGroup(id, out var group))
            return false;

        group.Rename(name);

        RaiseStateChanged();
        return true;
    }

    public bool TryChangeGroupTarget(
        int id,
        AudioGroupTarget target)
    {
        if (!TryGetGroup(id, out var group))
            return false;

        group.ChangeTarget(target);

        RaiseStateChanged();
        return true;
    }

    public bool TryUpdateGroup(
        int id,
        string name,
        AudioGroupTarget target)
    {
        if (!TryGetGroup(id, out var group))
            return false;

        group.Rename(name);
        group.ChangeTarget(target);

        RaiseStateChanged();
        return true;
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke();
    }
}
