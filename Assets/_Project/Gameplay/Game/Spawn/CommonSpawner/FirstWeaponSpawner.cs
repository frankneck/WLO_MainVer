using Unity.Entities;

public struct FirstWeaponsSpawnerTag : IComponentData { }

public struct ReadyToAddInContainer : IComponentData
{
    public Entity ContainerEntity;
}

public struct FirstWeaponsQuantity : IComponentData
{
    public int Value;
}