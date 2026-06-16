using Unity.Entities;

public struct PlayerFirstWeaponsSpawnerTag : IComponentData { }

/// <summary>
/// Marks that entity (e.g. Weapon item entity) is able to be added into container buffer 
/// </summary>
public struct AbleToAddIntoContainer : IComponentData
{
    public Entity ContainerEntity;
}

/// <summary>
/// Quantity of first player weapons on match|round start 
/// </summary>
public struct PlayerFirstWeaponsSpawnerQuantity : IComponentData
{
    public int Value;
}