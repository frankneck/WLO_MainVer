using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Request to load level on the client. Send to the server from client
/// </summary>
public partial struct ClientLevelLoadRequest : IRpcCommand { }

/// <summary>
/// Request to laod level on the client. Sned to the client from server
/// </summary>
public partial struct SendLevelRequest : IRpcCommand
{
    public int Index;
}