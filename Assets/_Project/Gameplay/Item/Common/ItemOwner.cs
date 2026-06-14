using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Marks who is owner of the item
/// </summary>
public struct ContainerEntityReference : IComponentData
{
    public Entity Entity;
}

