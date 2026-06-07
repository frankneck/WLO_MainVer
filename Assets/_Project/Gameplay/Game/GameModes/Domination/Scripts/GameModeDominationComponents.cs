using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Stores neccessary settings for Death Race. Attach to singleton
/// </summary>
[GhostComponent()]
public struct DominationMatchSettings : IComponentData
{
    [GhostField()] public int MaxPlayers;
    [GhostField()] public float MatchTime;
    [GhostField()] public float RevivalTime;
    [GhostField()] public int MaxScore;
}

[GhostComponent()]
public struct DominationPlayersData : IComponentData
{
    [GhostField()] public int PlayersNumber;
}

public struct DominationMatchTag : IComponentData { }