using Unity.Entities;

/// <summary>
/// Settings for developers
/// </summary>
public struct GameMatchGlobalSettings : IComponentData
{
    public int MinPlayersPerTeamToStartDeathmatch;
    public int MinPlayersToStartMatch;
    public int TimeBeforeStartingRound;
    public int TimeAfterFinishingRound;
    public int TimeAfterFinishingMatch;
}

public struct GameMatchGlobalSettingsTag : IComponentData { }