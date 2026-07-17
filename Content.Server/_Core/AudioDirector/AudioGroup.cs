using Content.Server._Core.AudioDirector.Targets;

namespace Content.Server._Core.AudioDirector;

public sealed class AudioGroup
{

    public int Id { get; }

    public string Name { get; private set; }

    public AudioGroupTarget Target { get; private set; }

    public AudioGroup(
        int id,
        string name,
        AudioGroupTarget target)
    {
        Id = id;
        Name = name;
        Target = target;
    }

    internal void Rename(string name)
    {
        Name = name;
    }

    internal void ChangeTarget(AudioGroupTarget target)
    {
        Target = target;
    }

}
