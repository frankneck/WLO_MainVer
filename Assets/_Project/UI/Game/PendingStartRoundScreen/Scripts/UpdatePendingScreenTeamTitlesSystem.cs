using Unity.Entities;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdatePendingScreenTeamTitlesSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PendingStartMatchScreen>();
    }

    protected override void OnUpdate()
    {
        var screenView = SystemAPI.ManagedAPI.GetSingleton<PendingStartMatchScreen>();

        foreach (var (matchTeams, matchEntity) in SystemAPI
            .Query<DeathmatchTeamsData>()
            .WithEntityAccess())
        {
            var settings = EntityManager.GetComponentData<DeathmatchMatchSettings>(matchEntity);
            
            screenView.UpdateRedTitle(matchTeams.RedPlayers, settings.MaxPlayersNumberPerTeam);
            screenView.UpdateBlueTitle(matchTeams.BluePlayers, settings.MaxPlayersNumberPerTeam);
        }
    }
}