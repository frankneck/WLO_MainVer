using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdatePendingScreenPlayerTitleSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PendingStartMatchScreen>();
    }

    protected override void OnUpdate()
    {
        var screenView = SystemAPI.ManagedAPI.GetSingleton<PendingStartMatchScreen>();

        foreach (var match in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>())
        {
            if (SystemAPI.HasComponent<DominationMatchTag>(match.Entity) &&
                SystemAPI.HasComponent<DominationMatchSettings>(match.Entity))
            {
                var settings = SystemAPI.GetComponent<DominationMatchSettings>(match.Entity);
                var players = SystemAPI.GetComponent<DominationPlayersData>(match.Entity);
                screenView.UpdatePlayersTitle(players.PlayersNumber, settings.MaxPlayers);
            }
        }
    }
} 