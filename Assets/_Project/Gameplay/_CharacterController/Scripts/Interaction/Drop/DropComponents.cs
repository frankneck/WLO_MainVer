using Unity.Entities;
using Unity.Mathematics;

public struct RemoveDroppedItemFromBuffer : IComponentData
{
    public Entity ContainerEntity;
    public int IndexInBuffer;
    public int ItemQuantity;
}

public struct DropItemRequest : IComponentData
{
    public Entity ItemEntity; 
    public Entity ContainerEntity;
    public quaternion Rot;
    public float3 Pos;
    public int IndexInBuffer;
    public int ItemQuantity;
} 