using Unity.Entities;
using Unity.Mathematics;

public struct TeamSpawnPointTag : IComponentData { }

/// <summary>
/// identifies spawner's team
/// </summary>
public struct PlayerSpawnPointTeam : IComponentData
{
    public TeamType Value;
}

public struct PlayerSpawnPointOffset : IBufferElementData
{
    public float3 Value;
} 