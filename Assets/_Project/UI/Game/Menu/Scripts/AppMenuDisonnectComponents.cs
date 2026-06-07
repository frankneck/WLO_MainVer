using Unity.Entities;
using Unity.NetCode;

public struct ClientOnDisconnectButtonRequest : IComponentData { }


public struct ClientDisconnectRpc : IRpcCommand
{
    public Entity Player;
}