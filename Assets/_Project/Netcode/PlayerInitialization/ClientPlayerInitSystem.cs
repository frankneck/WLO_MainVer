using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ClientInitSystem : ISystem
{
    private EntityQuery _pendingEntityWithIdQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var entityIdQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
        _pendingEntityWithIdQuery = state.GetEntityQuery(entityIdQueryBuilder);
        
        state.RequireForUpdate(_pendingEntityWithIdQuery);
        state.RequireForUpdate<ClientPlayerInitRequest>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        var initReq = SystemAPI.GetSingleton<ClientPlayerInitRequest>();
        var requestedTeam = initReq.Team;
        var requestedPlayerName = initReq.Nickname;

        var pendingsEntityWithIds = _pendingEntityWithIdQuery.ToEntityArray(Allocator.Temp);
        
        foreach (var pendingEntityWithkId in pendingsEntityWithIds)
        {
            Entity playerInitRpcEntity = ecb.CreateEntity();
            ecb.AddComponent(playerInitRpcEntity, new PlayerInitRequest { 
                TeamValue = requestedTeam, 
                PlayerName = requestedPlayerName
            });

            ecb.AddComponent(playerInitRpcEntity, new SendRpcCommandRequest { TargetConnection = pendingEntityWithkId });
            ecb.AddComponent<NetworkStreamInGame>(pendingEntityWithkId);
            
            UnityEngine.Debug.Log("[Client] Player data was sent to server.");
        }
        ecb.Playback(state.EntityManager);
    }
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ClientCameraIntiSystem : ISystem
{
    private EntityQuery _pendingCameraInitForGhost;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var entityIdQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<FirstPersonCharacterComponent, GhostOwnerIsLocal>().WithNone<LocalInitialized>();
        _pendingCameraInitForGhost = state.GetEntityQuery(entityIdQueryBuilder);
        
        state.RequireForUpdate(_pendingCameraInitForGhost);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        var characterEntities = _pendingCameraInitForGhost.ToEntityArray(Allocator.Temp);
        var characterComponents = _pendingCameraInitForGhost.ToComponentDataArray<FirstPersonCharacterComponent>(Allocator.Temp);
        
        for (int i = 0; i < characterEntities.Length; i++)
        {
            ecb.AddComponent(characterComponents[i].ViewEntity, new MainEntityCamera());
            ecb.AddComponent(characterEntities[i], new LocalInitialized());
            
            UnityEngine.Debug.Log("[Client] Player was finally initialized on client (camera initialized)");
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }
}