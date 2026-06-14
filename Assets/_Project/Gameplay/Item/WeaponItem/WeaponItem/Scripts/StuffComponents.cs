using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct SpellsInWeaponBuffer : IBufferElementData
{
    [GhostField] public Entity Entity;
    [GhostField] public int Count;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponCapacity : IComponentData
{
    [GhostField] public int Value;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponShuffle : IComponentData
{
    [GhostField] public bool Value;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponSpread : IComponentData
{
    [GhostField] public float Value;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponCastSpellNumber : IComponentData
{
    [GhostField] public int Value;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponCastDelay : IComponentData
{
    [GhostField] public float Value;
}

// This component stores data to choose new spell (if it isn't shuffle)
public struct StuffSpellState : IComponentData
{
    public Unity.Mathematics.Random Random;
    public int LastIndex;
}

// public struct StuffIsLocal : IComponentData { }

public struct SpawnSpellRequest : IComponentData
{
    public NetworkTick FireTick;
    public int Index;
}

public struct SpellCastRquest : IComponentData
{
    public Entity Player;
    public Entity Weapon;
    public int SpellIndex;
    public float ManaCost;
    public NetworkTick CooldownTick;
}

public struct NeedToInitSlots : IComponentData { }