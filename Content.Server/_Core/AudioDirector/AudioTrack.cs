namespace Content.Server._Core.AudioDirector;

public sealed class AudioTrack
{
    public int Id { get; }

    public string Name { get; private set; }

    public string Path { get; private set; }

    public float Volume { get; private set; }

    public bool Loop { get; private set; }

    public AudioTrack(
        int id,
        string name,
        string path,
        float volume,
        bool loop)
    {
        Id = id;
        Name = name;
        Path = path;
        Volume = volume;
        Loop = loop;
    }

    internal void Rename(string name)
    {
        Name = name;
    }

    internal void ChangePath(string path)
    {
        Path = path;
    }

    internal void ChangeVolume(float volume)
    {
        Volume = volume;
    }

    internal void ChangeLoop(bool loop)
    {
        Loop = loop;
    }
}
