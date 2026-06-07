using UnityEngine;

[CreateAssetMenu(fileName = "GameMatchGlobalSettingsConfig", menuName = "Match/New GameMatchGlobalSettingsConfig")]
public class GameMatchGlobalSettingsConfig : ScriptableObject
{
    [SerializeField] private int MinPlayersPerTeamToStartDeathmatch;
    [SerializeField] private int MinPlayersToStartDominationMatch;
    [SerializeField] private int TimeBeforeStartingRound;
    [SerializeField] private int TimeAfterFinishingRound;
    [SerializeField] private int TimeAfterFinishingMatch;

    public GlobalSettingsConfigData GetConfigData()
    {
        return new GlobalSettingsConfigData
        {
            MinPlayersPerTeamToStartMatch = MinPlayersPerTeamToStartDeathmatch,
            MinPlayersToStartDominationMatch = MinPlayersToStartDominationMatch,
            TimeBeforeStartingRound = TimeBeforeStartingRound,
            TimeAfterFinishingRound = TimeAfterFinishingRound,
            TimeAfterFinishingMatch = TimeAfterFinishingMatch
        };  
    }
}

public struct GlobalSettingsConfigData
{
    public int MinPlayersToStartDominationMatch;
    public int MinPlayersPerTeamToStartMatch;
    public int TimeBeforeStartingRound;
    public int TimeAfterFinishingRound;
    public int TimeAfterFinishingMatch;
}