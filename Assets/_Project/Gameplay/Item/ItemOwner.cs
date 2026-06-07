using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Marks who is owner of the item
/// </summary>
public struct ItemOwner : IComponentData
{
    public Entity Entity;
}

