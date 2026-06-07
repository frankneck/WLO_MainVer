using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Stores neccessary settings for Death match. Attach to singleton
/// </summary>
[GhostComponent()]
public struct DeathmatchMatchSettings : IComponentData
{
    [GhostField()] public int RoundsNumber;
    [GhostField()] public int MaxPlayersNumberPerTeam;
    [GhostField()] public float RoundTime;
}

/// <summary>
/// Number of played games. It consist from winner team entities
/// </summary>
[GhostComponent()]
public struct WinnderBuffer : IBufferElementData
{
    [GhostField()] 
    public TeamType WinnerTeam;
}

public struct DeathmatchMatchTag : IComponentData { }

/// <summary>
/// Marks that match entit created round entity
/// </summary>
public struct StartedRoundTag : IComponentData { }

/// <summary>
/// Tick when round will finish
/// </summary>
[GhostComponent()]
public struct RoundTimer : IComponentData
{
    [GhostField()] public NetworkTick Tick;
}

[GhostComponent()]
public struct LeftSecondsToFinishRoundTimer : IComponentData
{
    [GhostField()] public int Value;
} 