using Unity.Entities;
using Unity.NetCode;

// MAIN COMPONENTS

/// <summary>
/// Marks that Entity is a container. Add to Container Entity
/// </summary>
public struct ContainerTag :  IComponentData { }

/// <summary>
/// Data to initialize container. Add to Container Entity
/// </summary>
public struct InitContainerRequest : IComponentData
{
    public Entity Item;
    public Entity ItemContainer;
    public ContainerType Type;
    public int Size;
}

/// <summary>
/// Buffer stores Item and its count. Add to container Entity
/// </summary>
[GhostComponent]
public struct ContainerBuffer : IBufferElementData
{
    [GhostField()] public Entity ItemEntity;
    [GhostField()] public int Quantity;
}

/// <summary>
/// Marks that server container has local container link
/// </summary>
public struct EntityWithContainerTag : IComponentData { }

/// <summary>
/// Local container first initialization 
/// </summary>
public struct LocalContainerInitialized : IComponentData { }

/// <summary>
/// Defines type of the container. Add to container Entity
/// </summary>
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct ContainerTypeComponent : IComponentData
{
    [GhostField()] public ContainerType Value;
}

// PREPARING CONTAINERS for ENTITY

/// <summary>
/// Define Entity to create container or container in depends of having components (CharacterContainer, WeaponContainer etc.)
/// </summary>
public struct CreateContainerForEntityRequest : IComponentData
{
    public Entity Entity;
}

public struct NeedToCreateContainer : IEnableableComponent, IComponentData { }

public struct ContainerOwnerEntityReference : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// Ghost Item Containers for the Character Entity. Add to Character Entity
/// </summary>
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WithCharacterContainers : IComponentData
{
    [GhostField()] public Entity WeaponEquipmentContainer;
    [GhostField()] public Entity ConsumableEquipmentContainer;
    [GhostField()] public Entity InventoryContainer;
}


/// <summary>
/// Container for the Weapon Entity. Add to Weapon Entity
/// </summary>
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WithWeaponContainer : IComponentData
{
   [GhostField()] public Entity Container; 
}

// OTHERS

/// <summary>
/// Type of one container which define where item is placed
/// </summary>
public enum ContainerType : byte
{
    None = 0,
    CharacterWeaponEquipment,
    CharacterConsumableEquipment,
    CharacterInventory,
    Weapon,
    Storage,
}

/// <summary>
/// Keeps container version
/// </summary>
[GhostComponent()]
public struct ContainerVersion : IComponentData
{
    [GhostField()] public int Value;
}

/// <summary>
/// Marks that container need to update 
/// </summary>
public struct ContainerDirtyTag : IComponentData { }