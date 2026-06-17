using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Main system for client to load level
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientLoadLevelSystem : SystemBase
{
    private LevelLoaderSystem m_LevelLoader;

    protected override void OnCreate()
    {
        m_LevelLoader = World.GetExistingSystemManaged<LevelLoaderSystem>();
        
        RequireForUpdate<NetworkStreamConnection>();
        RequireForUpdate<CurrentLevelSyncState>();
        RequireForUpdate<LevelListData>();
    }

    protected override void OnUpdate()
    {
        var levelSyncState = SystemAPI.GetSingleton<CurrentLevelSyncState>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (rpc, receive, entity) in SystemAPI
            .Query<ClientLoadLevel, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            levelSyncState.State = LevelSyncState.LevelLoadRequest;
            levelSyncState.NextLevel = rpc.Index;
            SystemAPI.SetSingleton(levelSyncState);
            
#if UNITY_EDITOR
            var currentState = SystemAPI.GetSingleton<CurrentLevelSyncState>();        
            UnityEngine.Debug.Log($"[_ClientLoadLevelSystem] Current level sync state {currentState.State} and next level {currentState.NextLevel}");
#endif

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        if (levelSyncState.State == LevelSyncState.LevelLoadRequest)
        {
            m_LevelLoader.LoadLevel(levelSyncState.NextLevel);
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[_ClientLoadLevelSystem] Load level method called on client.");
#endif
        }
    }
}