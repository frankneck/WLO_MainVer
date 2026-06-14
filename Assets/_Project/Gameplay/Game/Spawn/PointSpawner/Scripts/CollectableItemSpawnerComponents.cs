using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct SpawnPointTransform : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
}

public struct WorldViewAnimaParameters : IComponentData
{
    public float BasePositionY;
    public float Amplitude;
    public float Scale;
    public float RotationSpeed;
    public bool Initialized;
}

public struct SpawnerCooldown : IComponentData
{
    public float Value; 
}

public struct SpawnerTargetTick : IComponentData
{
    public NetworkTick Tick;
}

public struct SpawnerInitialized : IComponentData { }

public struct AssignLevelRequest : IComponentData
{
    public Entity SpawnerEntity;
    public Entity SpawnedEntity;
    public WeaponLevel Level;
}

/// <summary>
/// Need to init for each parameter for spawned entity
/// </summary>
public struct SpawnerParamSet : IBufferElementData
{
    public ParameterId Id;
    public ParameterType Type;
    public float Threshold;
    public float Step;
    public float MinValue;
    public float MaxValue;
}