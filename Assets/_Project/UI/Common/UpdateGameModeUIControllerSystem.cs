using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class UpdateGameModeUIControllerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var match in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>())
        {
            if (SystemAPI.HasComponent<DeathmatchMatchTag>(match.Entity))
            {
                UIController.Instance.UpdateCurrentGameMode(GameMode.Deathmatch);
            }
            else if (SystemAPI.HasComponent<DominationMatchTag>(match.Entity))
            {
                UIController.Instance.UpdateCurrentGameMode(GameMode.Domination);
            }
        }
    }
}