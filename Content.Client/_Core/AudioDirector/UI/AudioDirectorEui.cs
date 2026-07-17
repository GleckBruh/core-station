using System.Linq;
using Content.Client.Eui;
using Content.Shared._Core.AudioDirector.Eui;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Shared.Network;

namespace Content.Client._Core.AudioDirector.UI;

[UsedImplicitly]
public sealed partial class AudioDirectorEui : BaseEui
{
    private readonly AudioDirectorWindow _window;

    private CreateAudioGroupWindow? _createGroupWindow;
    private DeleteAudioGroupWindow? _deleteGroupWindow;
    private EditAudioGroupWindow? _editGroupWindow;

    private AudioGroupEuiData[] _groups = [];
    private AudioDirectorPlayerEuiData[] _players = [];

    public AudioDirectorEui()
    {
        _window = new AudioDirectorWindow();

        _window.OnClose += SendClosedMessage;
        _window.OnAddGroupPressed += OpenCreateGroupWindow;
        _window.OnDeleteGroupPressed += OpenDeleteGroupConfirmation;
        _window.OnEditGroupPressed += OpenEditGroupWindow;
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
        _window.OnAddGroupPressed -= OpenCreateGroupWindow;
        _window.OnDeleteGroupPressed -= OpenDeleteGroupConfirmation;
        _window.OnEditGroupPressed -= OpenEditGroupWindow;

        _window.Close();
    }

    private void OpenCreateGroupWindow()
    {
        if (_createGroupWindow != null)
        {
            _createGroupWindow.MoveToFront();
            return;
        }

        _createGroupWindow = new CreateAudioGroupWindow(
            _players);

        _createGroupWindow.OnCreateGridGroupRequested +=
            SendCreateGridGroupRequest;

        _createGroupWindow.OnCreateMapGroupRequested +=
            SendCreateMapGroupRequest;

        _createGroupWindow.OnCreatePlayersGroupRequested +=
            SendCreatePlayersGroupRequest;

        _createGroupWindow.OnClose += () =>
            _createGroupWindow = null;

        _createGroupWindow.OpenCentered();
    }

    private void OpenEditGroupWindow(int groupId)
    {
        var group = _groups.FirstOrDefault(
            group => group.Id == groupId);

        if (group == null)
            return;

        if (_editGroupWindow != null)
        {
            _editGroupWindow.Close();
            _editGroupWindow = null;
        }

        _editGroupWindow = new EditAudioGroupWindow(
            group,
            _players);

        _editGroupWindow.OnSaveGridRequested +=
            SendUpdateGridGroupRequest;

        _editGroupWindow.OnSaveMapRequested +=
            SendUpdateMapGroupRequest;

        _editGroupWindow.OnSavePlayersRequested +=
            SendUpdatePlayersGroupRequest;

        _editGroupWindow.OnClose += () =>
            _editGroupWindow = null;

        _editGroupWindow.OpenCentered();
    }

    private void OpenDeleteGroupConfirmation(int groupId)
    {
        var group = _groups.FirstOrDefault(
            group => group.Id == groupId);

        if (group == null)
            return;

        if (_deleteGroupWindow != null)
        {
            _deleteGroupWindow.Close();
            _deleteGroupWindow = null;
        }

        _deleteGroupWindow = new DeleteAudioGroupWindow(
            group.Id,
            group.Name);

        _deleteGroupWindow.OnDeleteConfirmed +=
            SendDeleteGroupRequest;

        _deleteGroupWindow.OnClose += () =>
            _deleteGroupWindow = null;

        _deleteGroupWindow.OpenCentered();
    }

    private void SendCreateGridGroupRequest(
        string name,
        string gridNetEntityId)
    {
        SendMessage(
            new AudioDirectorEuiMsg.CreateGridGroupRequest(
                name,
                gridNetEntityId));
    }

    private void SendCreateMapGroupRequest(
        string name,
        int mapId)
    {
        SendMessage(
            new AudioDirectorEuiMsg.CreateMapGroupRequest(
                name,
                mapId));
    }

    private void SendCreatePlayersGroupRequest(
        string name,
        NetUserId[] players)
    {
        SendMessage(
            new AudioDirectorEuiMsg.CreatePlayersGroupRequest(
                name,
                players));
    }

    private void SendUpdateGridGroupRequest(
        int groupId,
        string name,
        string gridNetEntityId)
    {
        SendMessage(
            new AudioDirectorEuiMsg.UpdateGridGroupRequest(
                groupId,
                name,
                gridNetEntityId));
    }

    private void SendUpdateMapGroupRequest(
        int groupId,
        string name,
        int mapId)
    {
        SendMessage(
            new AudioDirectorEuiMsg.UpdateMapGroupRequest(
                groupId,
                name,
                mapId));
    }

    private void SendUpdatePlayersGroupRequest(
        int groupId,
        string name,
        NetUserId[] players)
    {
        SendMessage(
            new AudioDirectorEuiMsg.UpdatePlayersGroupRequest(
                groupId,
                name,
                players));
    }

    private void SendDeleteGroupRequest(int groupId)
    {
        SendMessage(
            new AudioDirectorEuiMsg.DeleteGroupRequest(
                groupId));
    }

    private void SendClosedMessage()
    {
        SendMessage(new CloseEuiMessage());
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if (state is not AudioDirectorEuiState audioState)
            return;

        _groups = audioState.Groups;
        _players = audioState.Players;

        _window.SetGroups(audioState.Groups);
        _createGroupWindow?.SetPlayers(_players);
        _editGroupWindow?.SetPlayers(_players);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case AudioDirectorEuiMsg.OperationSuccess success:
                HandleOperationSuccess(success);
                break;

            case AudioDirectorEuiMsg.OperationError error:
                HandleOperationError(error);
                break;
        }
    }

    private void HandleOperationSuccess(
        AudioDirectorEuiMsg.OperationSuccess success)
    {
        switch (success.Operation)
        {
            case AudioDirectorEuiMsg.AudioDirectorOperation.CreateGroup:
                _createGroupWindow?.Close();
                break;

            case AudioDirectorEuiMsg.AudioDirectorOperation.DeleteGroup:
                _deleteGroupWindow?.Close();
                break;

            case AudioDirectorEuiMsg.AudioDirectorOperation.UpdateGroup:
                _editGroupWindow?.Close();
                break;
        }
    }

    private void HandleOperationError(
        AudioDirectorEuiMsg.OperationError error)
    {
        switch (error.Operation)
        {
            case AudioDirectorEuiMsg.AudioDirectorOperation.CreateGroup:
                _createGroupWindow?.ShowError(error.Error);
                break;

            case AudioDirectorEuiMsg.AudioDirectorOperation.DeleteGroup:
                break;

            case AudioDirectorEuiMsg.AudioDirectorOperation.UpdateGroup:
                _editGroupWindow?.ShowError(error.Error);
                break;
        }
    }
}
