using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdatePlayersNumberSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<TeamSelectionScreen>();
    }

    protected override void OnUpdate()
    {
        TeamSelectionScreen teamSelection = SystemAPI.ManagedAPI.GetSingleton<TeamSelectionScreen>();

        foreach (var match in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>())
        {
            Entity matchEntity = match.Entity;

            if (!SystemAPI.HasComponent<DeathmatchTeamsData>(matchEntity))
                continue;
            
            var teams = SystemAPI.GetComponent<DeathmatchTeamsData>(matchEntity);

            teamSelection.UpdatePlayersNumber(teams.RedPlayers, teams.BluePlayers);
        }
    }
}

