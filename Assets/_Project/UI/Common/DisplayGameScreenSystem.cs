using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Display window depends of Current UI State
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class DisplayGameScreenSystem : SystemBase
{
    private PlayerState m_CashedState;

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // check that ui controller is created
        if (UIController.Instance == null)
            return;

        UIController.Instance.SetCommandBuffer(ref ecb);

        foreach (var (playerState, match) in SystemAPI
            .Query<CurrentPlayerState, BelongsToMatch>()
            .WithAll<GhostOwnerIsLocal>())
        {
            if (m_CashedState == playerState.Value)
                continue;

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

            m_CashedState = playerState.Value;
        }

        // Playback all recorded commands after iteration completes
        ecb.Playback(EntityManager);
    
        UIController.Instance.ClearCommandBuffer();
        ecb.Dispose();
    }
}