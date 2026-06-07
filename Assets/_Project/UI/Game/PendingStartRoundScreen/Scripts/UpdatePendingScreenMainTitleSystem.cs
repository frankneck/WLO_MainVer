using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateTitlesForDeathmatchPendingScreenSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PendingStartMatchScreen>();
        RequireForUpdate<GameMatchGlobalSettings>();
    }

    protected override void OnUpdate()
    {
        var globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();
        var screenView = SystemAPI.ManagedAPI.GetSingleton<PendingStartMatchScreen>();

        foreach (var (match, playerEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<GhostOwnerIsLocal>()
            .WithEntityAccess())
        {
            if (SystemAPI.HasComponent<DeathmatchMatchTag>(match.Entity))
            {
                // if deaathmatch
                var matchTeams = SystemAPI.GetComponent<DeathmatchTeamsData>(match.Entity);
                int totalPlayers = matchTeams.BluePlayers + matchTeams.RedPlayers;
                screenView.UpdateMainTitle(totalPlayers, globalSettings.MinPlayersPerTeamToStartDeathmatch * 2);
            }
            else if (SystemAPI.HasComponent<DominationMatchTag>(match.Entity))
            {
                // if domination
                var players = SystemAPI.GetComponent<DominationPlayersData>(match.Entity);
                screenView.UpdateMainTitle(players.PlayersNumber, globalSettings.MinPlayersToStartMatch);
            }
        }
    }
}

// 