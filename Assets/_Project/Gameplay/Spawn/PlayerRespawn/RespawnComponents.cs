using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

// Need to know who is respawning
public struct RespawnEntityTag : IComponentData { }

// Buffer wher we stored respawn entity 
public struct RespawnElementBuffer : IBufferElementData
{
    [GhostField] public NetworkTick RespawnTick;
    [GhostField] public Entity NetworkEntity;
    [GhostField] public NetworkId NetworkId;
}

// Current tick count to respawn entity
public struct RespawnTickCount : IComponentData
{
    public uint Value;
}

// Info about our player to a client
public struct PlayerSpawnInfo : IComponentData
{
    public TeamType Team;
    public FixedString128Bytes PlayerName;
    public float3 Position; 
    public quaternion Rotation;
}

// Entity reference to player to a client
[GhostComponent]
public struct NetworkEntityReference : IComponentData
{
    [GhostField] public Entity Entity;
}

// This entity need to assign controlled character
public struct PlayerEntityReference : IComponentData
{
    public Entity Entity;
}

// Server -> Client 
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct LeftSecondsToRespawn : IComponentData
{
    [GhostField] public int Value;
}

public struct AbleToAssignCharacter : IComponentData
{
    public Entity CharacterEntity;
}

/// <summary>
/// Stores data entity about Entity that need to respawn
/// </summary>
public partial struct AddCharacterIntoRespawnBuffer : IComponentData
{
    public Entity CharacterEntity;
}