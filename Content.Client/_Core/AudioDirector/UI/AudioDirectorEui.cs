using Content.Client.Eui;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._Core.AudioDirector.UI;

[UsedImplicitly]
public sealed partial class AudioDirectorEui : BaseEui
{
    private readonly AudioDirectorWindow _window;

    public AudioDirectorEui()
    {
        _window = new AudioDirectorWindow();
        _window.OnClose += SendClosedMessage;
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _window.OnClose -= SendClosedMessage;
        _window.Close();
    }

    private void SendClosedMessage()
    {
        SendMessage(new CloseEuiMessage());
    }
}
