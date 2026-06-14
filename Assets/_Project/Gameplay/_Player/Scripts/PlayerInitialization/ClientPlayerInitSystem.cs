using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
        var requestedGameMode = initReq.GameMode;

        var pendingsEntityWithIds = _pendingEntityWithIdQuery.ToEntityArray(Allocator.Temp);
        
        foreach (var pendingEntityWithkId in pendingsEntityWithIds)
        {
            Entity playerInitRpcEntity = ecb.CreateEntity();
            ecb.AddComponent(playerInitRpcEntity, new PlayerInitRequest { 
                GameMode = requestedGameMode,
                TeamValue = requestedTeam, 
                PlayerName = requestedPlayerName
            });

            ecb.AddComponent(playerInitRpcEntity, new SendRpcCommandRequest { TargetConnection = pendingEntityWithkId });
            ecb.AddComponent<NetworkStreamInGame>(pendingEntityWithkId);

#if UNITY_EDITOR
            UnityEngine.Debug.Log("[Client] Player data was sent to server.");
#endif
        }
        ecb.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ClientCameraIntiSystem : ISystem
{
    private EntityQuery _pendingCameraInitForGhost;

    public void OnCreate(ref SystemState state)
    {
        var entityIdQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<FirstPersonCharacterComponent, FirstPersonCharacterViewReference, GhostOwnerIsLocal>().WithNone<LocalInitialized>();
        _pendingCameraInitForGhost = state.GetEntityQuery(entityIdQueryBuilder);
        
        state.RequireForUpdate(_pendingCameraInitForGhost);
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
                
        var characterEntities = _pendingCameraInitForGhost.ToEntityArray(Allocator.Temp);
        var characterComponents = _pendingCameraInitForGhost.ToComponentDataArray<FirstPersonCharacterViewReference>(Allocator.Temp);
        
        for (int i = 0; i < characterEntities.Length; i++)
        {
            var viewEntity = characterComponents[i].ViewEntity;

            ecb.AddComponent(viewEntity, new MainCameraEntity
            {
                Character = characterEntities[i]
            });

            // state.EntityManager.AddComponentObject(viewEntity, new MainCamera { Camera = Camera.main });
            
            ecb.AddComponent(characterEntities[i], new LocalInitialized());
            ecb.AddComponent(characterEntities[i], new LocalCharacterTag {});

#if UNITY_EDITOR
            UnityEngine.Debug.Log("[Client] Player was finally initialized on client (camera initialized)");
#endif
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct CleanupCameraEntitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (targetEntity, entity) in SystemAPI
            .Query<MainCameraEntity>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(targetEntity.Character))
                ecb.DestroyEntity(entity);
        }
    }
}