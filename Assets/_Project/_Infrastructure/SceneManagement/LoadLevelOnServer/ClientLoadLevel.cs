using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Notifies the clients what level need to load
/// </summary>
public partial struct ClientLoadLevel : IRpcCommand
{
    public int Index;
}

/// <summary>
/// Marks that connection has been initialized
/// </summary>
public partial struct ConnectionInitialized : IComponentData { }