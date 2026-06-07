using Unity.Entities;
using Unity.NetCode;

// ITEM TAGS

public struct WeaponTag : IComponentData { }
public struct SpellTag : IComponentData { }

// ITEM STATE

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct CurrentItemState : IComponentData
{
    [GhostField] public ItemState Value;
}

public struct ChangeCurrentItemState : IComponentData
{
    public ItemState NewState;
    public Entity ItemEntity;
}

public enum ItemState : byte
{
    World = 1,
    InContainer = 2,
    Equiped = 3,
}

// BUFFERS

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct EquipmentBuffer : IBufferElementData
{
    [GhostField] public Entity Item;
    [GhostField] public int Count;
} 

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct InventoryBuffer : IBufferElementData
{
    [GhostField] public Entity Entity;
    [GhostField] public int Count;
}

// BUFFER SIZEs

public struct EquipmentSize : IComponentData
{
    public int Value;
}

public struct InventorySize : IComponentData
{
    public int Value;
}

// REQUESTS

public struct ChangeItemStateRpc : IRpcCommand
{
    public Entity Item;
    public ItemAction Type;
}

public struct ClientChangeItemState : IComponentData
{
    public Entity Item;
    public ItemAction Type;
}

public enum ItemAction : byte
{
    Pickup = 1,
    Drop = 2,
    Equip = 3,
    InInventory = 4
}

// This need to AUTO FILLING INVENTORY and EQUIPMENT BUFFER of EACH PLAYER when it has joined 