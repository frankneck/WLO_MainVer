using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UIElements;

/// <summary>
/// Defines entity with healthbar 
/// </summary>
public struct EntityWithWorldUITag : IComponentData { }

/// <summary>
/// Stores target entity for world ui entity
/// </summary>
public struct WorldUITargetEntity : IComponentData
{
    public Entity Entity;
}

public struct WorldUIHeightOffset : IComponentData
{
    public float Value;
}

public struct CashedWorldUITargetEntityInfo : IComponentData
{
    public float3 Position;
    public FixedString128Bytes Name; 
    public StyleLength FillLength;
    public bool IsVisible;
}

public struct WorldSpaceControllerData : IComponentData
{
    public float MaxDistance;
}