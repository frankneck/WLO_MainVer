using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Display window depends of Current UI State
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class DisplayGameScreenSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (playerState, match) in SystemAPI
            .Query<CurrentPlayerState, BelongsToMatch>()
            .WithAll<GhostOwnerIsLocal>())
        {
            switch (playerState.Value)
            {                            
                case PlayerState.PendingStartMatch :
                    UIController.Instance.OnStartMatchCalled();
                    break;

                case PlayerState.Playing :
                    UIController.Instance.OnActiveMatchCalled();
                    break;

                case PlayerState.FinishingMatch :
                    UIController.Instance.OnFinishMatchCalled();
                    break;

                case PlayerState.Dead :
                    UIController.Instance.OnPlayerDeadCalled();
                    break;

                case PlayerState.Respawning :
                    UIController.Instance.OnPlayerRespawningCalled();
                    break;

                case PlayerState.PendingStartRound :
                    UIController.Instance.OnStartRoundCalled();
                    break;
                
                case PlayerState.PendingFinishRound :
                    UIController.Instance.OnFinishRoundCalled();
                    break;

                case PlayerState.Spectating :
                    UIController.Instance.OnSpectating();
                    break;

                case PlayerState.SelctingTeam :
                    UIController.Instance.OnTeamSelectionCalled();
                    break;
            }
        }        
    }
}