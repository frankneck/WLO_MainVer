using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateRoundTimerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
        HudScreen hudView = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();

        foreach (var belongsToMatch in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>())
        {
            Entity playerMatchEntity = belongsToMatch.Entity;

            if (!SystemAPI.HasComponent<LeftSecondsToFinishRoundTimer>(playerMatchEntity))
                continue;

            var seconds = SystemAPI.GetComponent<LeftSecondsToFinishRoundTimer>(playerMatchEntity).Value;
            hudView.UpdateFinishRoundTimer(seconds);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateDeathmatchGameStatsSystem : SystemBase
{
    private int m_RedWinsCashed;
    private int m_BlueWindsCashed;

    private int m_PlayedMatchCashed;

    protected override void OnCreate()
    {
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
        HudScreen hudView = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();

        foreach (var match in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>())
        {
            if (!SystemAPI.HasComponent<DeathmatchTeamsData>(match.Entity) ||
                !SystemAPI.HasComponent<DeathmatchMatchSettings>(match.Entity) || 
                !SystemAPI.HasComponent<PlayedRoundsNumber>(match.Entity))
                continue;

            var teams = SystemAPI.GetComponent<DeathmatchTeamsData>(match.Entity);

            var deathmachSettings = SystemAPI.GetComponent<DeathmatchMatchSettings>(match.Entity);
            var playedRoundsNumber = SystemAPI.GetComponent<PlayedRoundsNumber>(match.Entity).Value;

            if (playedRoundsNumber > 0 && m_PlayedMatchCashed != playedRoundsNumber)
            {
                hudView.UpdatePlayedRounds(
                    maxRounds: deathmachSettings.RoundsNumber,
                    playedRounds: playedRoundsNumber
                );

                m_PlayedMatchCashed = playedRoundsNumber;
            }                

            if (m_RedWinsCashed != teams.RedPlayersWins || 
                m_BlueWindsCashed != teams.BluePlayersWins)
            {
                hudView.UpdateDeathmatchStatistics(
                    redWinds: teams.RedPlayersWins, 
                    blueWins: teams.BluePlayersWins
                );
                
                m_RedWinsCashed = teams.RedPlayersWins;
                m_BlueWindsCashed = teams.BluePlayersWins;
            }
        }
    }
}