using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Entry point for changing current state on server.
/// Updates current player state
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct UpdateCurrentPlayerStateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>() 
            .CreateCommandBuffer(state.WorldUnmanaged);

        var jobHandle = new UpdateCurrentPlayerStateJob
        {
            CurrentPlayerStateLookup = SystemAPI.GetComponentLookup<CurrentPlayerState>(),
            NetworkEntityReferenceLookup = SystemAPI.GetComponentLookup<NetworkEntityReference>(),
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct UpdateCurrentPlayerStateJob : IJobEntity
{
    public ComponentLookup<CurrentPlayerState> CurrentPlayerStateLookup; 
    public ComponentLookup<NetworkEntityReference> NetworkEntityReferenceLookup;
    public EntityCommandBuffer ECB;

    public void Execute(
        UpdateCurrentPlayerState request,
        Entity entity
    )
    {
        UnityEngine.Debug.Log($"Update current player state on {request.NewState}");

        Entity playerEntity = request.PlayerEntity;
        
        ECB.SetComponent(playerEntity, new CurrentPlayerState 
        { 
            Value = request.NewState
        });

        if (request.NewState == PlayerState.PendingStartMatch ||
            request.NewState == PlayerState.Spectating)
        {
            Entity connectionEntity = NetworkEntityReferenceLookup[playerEntity].Entity;

            var rpcEntity = ECB.CreateEntity();
            ECB.AddComponent<PlayerCharacterSpawned>(rpcEntity);
            ECB.AddComponent(rpcEntity, new SendRpcCommandRequest
            {
                TargetConnection = connectionEntity
            });
        }

        ECB.DestroyEntity(entity);
    }
}

public struct UpdateCurrentPlayerState : IComponentData
{
    public Entity PlayerEntity;
    public PlayerState NewState; 
}