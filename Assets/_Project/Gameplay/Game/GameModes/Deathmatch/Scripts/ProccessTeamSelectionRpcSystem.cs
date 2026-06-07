using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ProccessTeamSelectionRpcSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (rpc, recieve, rpcEntity) in SystemAPI
            .Query<TryJoinPlayerToTeamRpc, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            UnityEngine.Debug.Log($"[ProccessTeamSelectionRpcSystem] RPC Received.");

            Entity playerEntity = rpc.PlayerEntity;

            Entity matchEntity =  SystemAPI.GetComponent<BelongsToMatch>(playerEntity).Entity;

            GameTeam currentPlayerTeam = SystemAPI.GetComponent<GameTeam>(playerEntity);

            // Check if current player is already added to requested team
            if (currentPlayerTeam.Value == rpc.PlayerTeam)
            {
                UnityEngine.Debug.Log($"[ProccessTeamSelectionRpcSystem] Player is alredy in {rpc.PlayerTeam}.");
                ecb.DestroyEntity(rpcEntity);
                return;
            }

            var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(matchEntity);

            DeathmatchMatchSettings settings = SystemAPI.GetComponent<DeathmatchMatchSettings>(matchEntity);

            if (rpc.PlayerTeam == TeamType.Red && 
                teams.ValueRW.RedPlayers < settings.MaxPlayersNumberPerTeam)
            {
                teams.ValueRW.RedPlayers++;
            }
            else if (rpc.PlayerTeam == TeamType.Blue &&
                teams.ValueRW.BluePlayers < settings.MaxPlayersNumberPerTeam)
            {
                teams.ValueRW.BluePlayers++;
            }
            else
            {
                UnityEngine.Debug.Log("Current team is full.");
                ecb.DestroyEntity(rpcEntity);
                
                return;
            }

            ecb.SetComponent(playerEntity, new GameTeam
            {
                Value = rpc.PlayerTeam
            }); 

            if (SystemAPI.HasComponent<ActiveMatchTag>(matchEntity))
            {
                PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                    ref ecb, 
                    playerEntity, 
                    PlayerState.Spectating
                );
            }
            else
            {
                PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                    ref ecb, 
                    playerEntity, 
                    PlayerState.PendingStartMatch
                );
            }

            ecb.AddComponent<AblePlayerToStartRoundTag>(playerEntity);

            ecb.DestroyEntity(rpcEntity);
        }
    }  
}

public struct TeamPlayerTag : IComponentData { }

// Принимаем запрос
// Проходим по сущностям команды, которые относятся к текущему матчу (матч из игрока)
// Отправляем запрос на присоединение в команду, если текущее кол-во игроков в команде не больше макс.
// В случае неудачи просто уничтожаем rpc
