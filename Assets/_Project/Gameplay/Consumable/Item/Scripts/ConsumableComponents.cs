using Unity.Entities;

public struct ConsumableTag : IComponentData { }

public struct ConsumableTypeComponent : IComponentData
{
    public ConsumableType Value;
}

public struct SpendConsumable : IComponentData
{
    public Entity ConsumableItemEntity;
    public Entity CharacterEntity;
}