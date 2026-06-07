using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerInitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (recieveRPC, initRequest, entity) in SystemAPI
            .Query<ReceiveRpcCommandRequest, PlayerInitRequest>()
            .WithEntityAccess())
        {
            var requestedTeam = initRequest.TeamValue;
            var playerName = initRequest.PlayerName;
            var gameMode = initRequest.GameMode;

            ecb.AddComponent(recieveRPC.SourceConnection, new ServerPlayerInitRequest { 
                PlayerName = playerName, 
                GameMode = gameMode,
                TeamValue =  requestedTeam,
            });

            ecb.DestroyEntity(entity);
            UnityEngine.Debug.Log("[Server] Player has been initialized.");
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private TeamType GetAutoTeam(ref SystemState state)
    {
        // TODO: Assigning auto team
        return TeamType.Blue;
    }
}