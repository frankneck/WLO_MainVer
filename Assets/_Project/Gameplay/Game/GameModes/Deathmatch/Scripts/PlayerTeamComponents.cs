using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Value to win in match. It can be attached to Player character or Team in depends of game mode.
/// </summary>
[GhostComponent()]
public struct MatchScore : IComponentData
{
    [GhostField()] public int Value;
}

public struct PlayerTeamTag : IComponentData { }

/// <summary>
/// Main information about Team
/// </summary>
[GhostComponent()]
public struct PlayerTeamData : IBufferElementData
{
    [GhostField()] public TeamType TeamType;
    [GhostField()] public int CurrentPlayersNumber;
}

[GhostComponent()]
public struct DeathmatchTeamsData : IComponentData
{
    [GhostField()] public int RedPlayers;
    [GhostField()] public int RedPlayersAlive;
    [GhostField()] public int RedPlayersWins;
    
    [GhostField()] public int BluePlayers;
    [GhostField()] public int BluePlayersAlive;
    [GhostField()] public int BluePlayersWins;
}

[GhostComponent()]
public struct PlayedRoundsNumber : IComponentData
{
    [GhostField()] public int Value;
}