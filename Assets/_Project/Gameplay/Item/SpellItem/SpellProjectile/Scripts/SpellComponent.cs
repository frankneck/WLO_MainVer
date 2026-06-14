using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics.Extensions;

public struct ProjectileMoveSpeed : IComponentData
{
    public float Value;
}

public struct ManaCost : IComponentData
{
    public float Value;
}

public struct SpellTypeComponent : IComponentData
{
    public SpellType Value;
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
public struct ProjectileOwner : IComponentData
{
    public Entity Entity;
}

public enum SpellType : byte
{
    AoeSpell = 1,
    SkillShot = 2,
    None = 0,
}

public struct ProjectileDistance : IComponentData
{
    public float Value;
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
public struct ProjectileReference : IComponentData
{
    public Entity PrefabEntity;
}