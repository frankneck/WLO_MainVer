using Unity.Burst;
using Unity.Entities;

/// <summary>
/// Updates permissions to read inputs. 
/// For example, if player in selection of Team, he can only input ECS and Player list. 
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct UpdatePlayerPermissionsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var jobHandle = new UpdatePlayerPermissionsJob();
        state.Dependency = jobHandle.ScheduleParallel(state.Dependency);
    }   
}

[BurstCompile]
public partial struct UpdatePlayerPermissionsJob : IJobEntity
{
    public void Execute(
        ref InputPermissions permissions,
        in CurrentPlayerState playerState
    )
    {
        switch(playerState.Value)
        {
            case PlayerState.SelctingTeam:
                permissions.Value = 
                    InputFlags.Menu |
                    InputFlags.PlayerList;
                    break;

            case PlayerState.PendingStartRound:
                permissions.Value = 
                    InputFlags.Menu |
                    InputFlags.PlayerList |
                    InputFlags.Look;
                    break;

            case PlayerState.Playing:
                permissions.Value =
                    InputFlags.Move |
                    InputFlags.Look |
                    InputFlags.Shoot |
                    InputFlags.Inventory |
                    InputFlags.Menu | 
                    InputFlags.Interact |
                    InputFlags.Drop;
                    break;

            case PlayerState.Respawning:
                permissions.Value = InputFlags.None;
                    break;

            case PlayerState.Dead:
                permissions.Value = 
                    InputFlags.PlayerList |
                    InputFlags.Menu;
                    break;

            case PlayerState.None:
                permissions.Value = InputFlags.None;
                    break;
        }
    }
}
