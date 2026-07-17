using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Core.AudioDirector.Eui;

public static class AudioDirectorEuiMsg
{
    public enum AudioDirectorOperation
    {
        CreateGroup,
        DeleteGroup,
        UpdateGroup
    }

    [Serializable, NetSerializable]
    public sealed class CreateGridGroupRequest : EuiMessageBase
    {
        public string Name { get; }
        public string GridNetEntityId { get; }

        public CreateGridGroupRequest(
            string name,
            string gridNetEntityId)
        {
            Name = name;
            GridNetEntityId = gridNetEntityId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CreateMapGroupRequest : EuiMessageBase
    {
        public string Name { get; }
        public int MapId { get; }

        public CreateMapGroupRequest(
            string name,
            int mapId)
        {
            Name = name;
            MapId = mapId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CreatePlayersGroupRequest : EuiMessageBase
    {
        public string Name { get; }
        public NetUserId[] Players { get; }

        public CreatePlayersGroupRequest(
            string name,
            NetUserId[] players)
        {
            Name = name;
            Players = players;
        }
    }

    [Serializable, NetSerializable]
    public sealed class UpdateGridGroupRequest : EuiMessageBase
    {
        public int GroupId { get; }
        public string Name { get; }
        public string GridNetEntityId { get; }

        public UpdateGridGroupRequest(
            int groupId,
            string name,
            string gridNetEntityId)
        {
            GroupId = groupId;
            Name = name;
            GridNetEntityId = gridNetEntityId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class UpdateMapGroupRequest : EuiMessageBase
    {
        public int GroupId { get; }
        public string Name { get; }
        public int MapId { get; }

        public UpdateMapGroupRequest(
            int groupId,
            string name,
            int mapId)
        {
            GroupId = groupId;
            Name = name;
            MapId = mapId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class UpdatePlayersGroupRequest : EuiMessageBase
    {
        public int GroupId { get; }
        public string Name { get; }
        public NetUserId[] Players { get; }

        public UpdatePlayersGroupRequest(
            int groupId,
            string name,
            NetUserId[] players)
        {
            GroupId = groupId;
            Name = name;
            Players = players;
        }
    }

    [Serializable, NetSerializable]
    public sealed class DeleteGroupRequest : EuiMessageBase
    {
        public int GroupId { get; }

        public DeleteGroupRequest(int groupId)
        {
            GroupId = groupId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class OperationSuccess : EuiMessageBase
    {
        public AudioDirectorOperation Operation { get; }

        public OperationSuccess(
            AudioDirectorOperation operation)
        {
            Operation = operation;
        }
    }

    [Serializable, NetSerializable]
    public sealed class OperationError : EuiMessageBase
    {
        public AudioDirectorOperation Operation { get; }
        public string Error { get; }

        public OperationError(
            AudioDirectorOperation operation,
            string error)
        {
            Operation = operation;
            Error = error;
        }
    }
}
