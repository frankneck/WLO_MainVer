using System.Collections.Generic;
using Unity.Entities;


// Only client
public struct OpenInventoryRequest : IComponentData { }

/// <summary>
/// Stores character containers to create in SnapshotModel ModelContainers 
/// </summary>
public struct UpdateUIInventory : IComponentData
{
    public Entity InventoryContainer;
    public Entity WeaponEquipmentContainer;
    public Entity ConsumableEquipmentContainer;
}

public class InventoryCommandBuffer
{
    private readonly List<InventoryCommand> _commands = new();
    public IReadOnlyList<InventoryCommand> Commands => _commands;
    
    public bool Add(InventoryCommand command)
    {
        _commands.Add(command);
        return true;
    }

    public void Clear()
    {
        _commands.Clear();
    }
}

public struct InventoryCommand
{
    public Entity SourceOwner;
    public SlotType SourceType;
    public int SourceIndex;
    public Entity TargetOwner;
    public SlotType TargetType;
    public int TargetIndex;
    
    // TODO: Count
}
