using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct ItemViews : IComponentData
{
    public Entity WorldViewPrefab;
    public Entity FirstPersonViewPrefab;
    public Entity ThirdPersonViewPrefab;
}

/// <summary>
/// Stores last view entity to destroy when it need
/// </summary>
public struct LastViewEntity : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// Indicates what a view
/// </summary>
public struct ItemViewOwner : IComponentData
{
    public Entity Entity;
}

public struct ItemViewTransform : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
}

/// <summary>
/// Entity who equiped Item 
/// </summary>
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct EquipedBy : IComponentData
{
    [GhostField] public Entity Entity; 
}

/// <summary>
/// Link to character to display FP and TP Views
/// </summary>
public struct AttachedToCharacter : IComponentData
{
    public Entity Entity;
}

public struct WorldViewTag : IComponentData { }
public struct FirstPersonViewTag : IComponentData { }
public struct ThirdPersonViewTag : IComponentData { }