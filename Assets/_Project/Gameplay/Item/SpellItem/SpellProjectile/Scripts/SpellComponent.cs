using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct ProjectileMoveSpeed : IComponentData
{
    public float Value;
}

/// <summary>
/// Mana cost to cast spell
/// </summary>
[GhostComponent]
public struct ManaCost : IComponentData
{
    [GhostField(Quantization = 0)] public float Value;
}

/// <summary>
/// Type of spell. Need to define projectile behaviour
/// </summary>
[GhostComponent]
public struct SpellTypeComponent : IComponentData
{
    [GhostField] public SpellType Value;
}

/// <summary>
/// Distance where projectile can be moved or be casted
/// </summary>
[GhostComponent]
public struct SpellDistance : IComponentData
{
    [GhostField(Quantization = 0)] public float Value;
}

// Data to store in stuff inventory
public struct ShieldPrefab : IComponentData
{
    public Entity FireballItem;
    public Entity TrapAoeItem;
    public Entity Shield;
}

public struct SpellDirection : IComponentData
{
    public float3 Value;
}

// Need to know who cast this spell (SpawnSpellSystem)
public struct ProjectileCasterEntityReference : IComponentData
{
    public Entity Entity;
}

public enum SpellType : byte
{
    StaticProjectile = 1,
    MovingProjectile = 2,
    None = 0,
}

// AOE Trap
public struct JellyZone : IComponentData
{
    public float SpeedMultiplier;
    public float SharpnessMultiplier;
    public float AirAccelerationMultiplier;
    public float AirMaxSpeedMultiplier;
    public float AirDragMultiplier;
    public float GravityMultiplier;
    public float JumpMultiplier;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct ProjectileEntityReference : IComponentData
{
    public Entity PrefabEntity;
}