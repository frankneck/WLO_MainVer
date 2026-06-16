using Unity.Entities;

/// <summary>
/// Marks spawner
/// </summary>
public struct SpawnerTag : IComponentData { }

/// <summary>
/// What entity spawner must spawn
/// </summary>
public struct SpawnerTargetEntity : IComponentData
{
    public Entity PrefabEntity;
}

/// <summary>
/// What entity spawns target entity
/// </summary>
public struct SpawnerEntityReference : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// Tick when spawner shoud spawn entity
/// </summary>
public struct UpdateSpawnerTargetTick : IComponentData
{
    public Entity Spawner;
}

/// <summary>
/// Number of entities that spawner must spawn
/// </summary>
public struct NumberEntitiesToSpawn : IComponentData
{
    public int Value;
}

/// <summary>
/// Radius of spawn entity
/// </summary>
public struct SpawnRadius : IComponentData
{
    public float Value;
}

/// <summary>
/// Random distance radius
/// </summary>
public struct RadiusRandom : IComponentData
{
    public Unity.Mathematics.Random Value;
}

/// <summary>
/// Shows current mode of spawner. E.g. If SpawnOnce it means spawn one time (TODO: Change it or cut out)
/// </summary>
public struct CurrentSpawnerMode : IComponentData
{
    public SpawnerMode Value;
}

/// <summary>
/// Current spanw state. E.g. when Active is spawning, when Disactive - nothing
/// </summary>
public struct CurrentSpawnerState : IComponentData
{
    public SpawnerState Value;
}

public struct SpawnerWeaponLevel : IComponentData
{
    public WeaponLevel Value;
}

public enum SpawnerState : byte
{
    Active,
    Disactive
}

/// <summary>
/// Keeps spawner mode 
/// </summary>
public enum SpawnerMode : byte
{
    SpawnOneTime,
    SpawnAlways
}