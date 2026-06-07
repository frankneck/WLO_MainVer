using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ProcessClientDisconnectRpcSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (rpc, receive, rpcEntity) in SystemAPI
            .Query<ClientDisconnectRpc, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            Entity playerEntity = rpc.Player;

            if (!SystemAPI.HasComponent<BelongsToMatch>(playerEntity) ||
                !SystemAPI.HasComponent<GameTeam>(playerEntity))
            {
                ecb.DestroyEntity(rpcEntity);
                continue;
            }

            Entity matchEntity = SystemAPI.GetComponent<BelongsToMatch>(playerEntity).Entity;

            GameTeam team = SystemAPI.GetComponent<GameTeam>(playerEntity);

            if (SystemAPI.HasComponent<DeathmatchTeamsData>(matchEntity))
            {
                var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(matchEntity);

                if (team.Value == TeamType.Red)
                {
                    teams.ValueRW.RedPlayers--;
                    teams.ValueRW.RedPlayersAlive--;
                }
                else if (team.Value == TeamType.Blue)
                {
                    teams.ValueRW.BluePlayers--;
                    teams.ValueRW.BluePlayersAlive--;
                }
            }
            else if (SystemAPI.HasComponent<DominationPlayersData>(matchEntity))
            {
                var players = SystemAPI.GetComponentRW<DominationPlayersData>(matchEntity);
                players.ValueRW.PlayersNumber--;
            }

            UnityEngine.Debug.Log("Disconnect: Receive rpc and send back");

            var disconnectRpc = ecb.CreateEntity();
            ecb.AddComponent<DisconnectPlayer>(disconnectRpc);
            ecb.AddComponent(disconnectRpc, new SendRpcCommandRequest 
            { 
                TargetConnection = receive.SourceConnection 
            });

            // удаляем RPC entity
            ecb.DestroyEntity(rpcEntity);
        }
    }
}
