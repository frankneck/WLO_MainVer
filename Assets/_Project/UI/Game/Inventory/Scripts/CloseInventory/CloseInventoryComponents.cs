using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

/// <summary>
/// Marks when we close inventory
/// </summary>
public struct CloseInventoryRequest : IComponentData { }

public struct RpcInventoryCommands : IRpcCommand
{
    public FixedList512Bytes<InventoryCommand> Commands;
}