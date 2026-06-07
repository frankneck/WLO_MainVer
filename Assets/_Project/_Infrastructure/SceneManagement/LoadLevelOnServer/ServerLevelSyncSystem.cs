using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Creates LevelSyncStateComponent on the Server, handles all players that ready to play (loaded levels)
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerLevelSyncSystem : SystemBase
{
    protected override void OnCreate()
    {
        if (!SystemAPI.TryGetSingleton<LevelSyncStateComponent>(out var levelSyncState))
        {
            var entity = EntityManager.CreateEntity(typeof(LevelSyncStateComponent));
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[_ServerLevelSyncSystem] Tracked subscenes added as buffer");
#endif
            EntityManager.AddBuffer<TrackedSubscenes>(entity);
        }

        RequireForUpdate<LevelSyncStateComponent>();    
    }

    protected override void OnUpdate()
    {
        var levelSyncState = SystemAPI.GetSingleton<LevelSyncStateComponent>();
        
        // Order for all clients what level need to load when they're connected
        var connectionsQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkId>());
        var connectionEntities = connectionsQuery.ToEntityArray(Allocator.Temp);
        
        foreach (var connection in connectionEntities)
        {
            if (levelSyncState.State == LevelSyncState.Idle)
            {
                if (SystemAPI.HasComponent<ConnectionInitialized>(connection))
                    continue;
                
                EntityManager.AddComponent<ConnectionInitialized>(connection);
                SendClientLoadLevelRequest(connection, levelSyncState.CurrentLevel);
            }
            
            if (levelSyncState.State == LevelSyncState.LevelLoaded)
            {
                SendClientLoadLevelRequest(connection, levelSyncState.CurrentLevel);
            }
        }
        
        levelSyncState.State = LevelSyncState.Idle;
        SystemAPI.SetSingleton(levelSyncState);
        
// #if UNITY_EDITOR
//         var currentState = SystemAPI.GetSingleton<LevelSyncStateComponent>();
//         UnityEngine.Debug.Log($"[_ServerLevelSyncSystem] Set level sync state ({currentState.State}) and current level equals {currentState.CurrentLevel} on {World.Name}.");
// #endif
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        // for spawn character player
        foreach(var (rpc, receive, entity) in SystemAPI
            .Query<ClientReady, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<NetworkStreamInGame>(receive.SourceConnection))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[_ServerLevelSyncSystem] Added NetworkStreamInGame to connection entity {receive.SourceConnection}.");
#endif
                ecb.AddComponent<ReadyPlayerCharacterSpawn>(receive.SourceConnection);
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    private void SendClientLoadLevelRequest(Entity connection, int currentLevel)
    {
        var rpc = EntityManager.CreateEntity();
        EntityManager.AddComponentData(rpc, new ClientLoadLevel { Index = currentLevel });
        EntityManager.AddComponentData(rpc, new SendRpcCommandRequest { TargetConnection = connection });
        
        if (SystemAPI.HasComponent<NetworkStreamInGame>(connection))
        {
            EntityManager.RemoveComponent<NetworkStreamInGame>(connection);
        }
    }
}