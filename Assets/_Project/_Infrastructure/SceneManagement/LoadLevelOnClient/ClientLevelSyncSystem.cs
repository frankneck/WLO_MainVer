using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Creates LevelSyncStateComponent on the Server and marks server that client is ready to play
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientLevelSyncSystem : SystemBase
{
    protected override void OnCreate()
    {
        if (!SystemAPI.TryGetSingleton<CurrentLevelSyncState>(out var levelSyncState))
        {
            var entity = EntityManager.CreateEntity(typeof(CurrentLevelSyncState));
            UnityEngine.Debug.Log("[LevelLoader] Tracked subscenes added as buffer");
            EntityManager.AddBuffer<TrackedSubscenes>(entity);
        }

        RequireForUpdate<NetworkId>();
        RequireForUpdate<CurrentLevelSyncState>();
    }

    protected override void OnUpdate()
    {
        var levelSyncState = SystemAPI.GetSingleton<CurrentLevelSyncState>();
        var connection = SystemAPI.GetSingletonEntity<NetworkId>();

        if (levelSyncState.State == LevelSyncState.LevelLoaded)
        {
            var rpc = EntityManager.CreateEntity();
            EntityManager.AddComponentData(rpc, new SendRpcCommandRequest { TargetConnection = connection });
            EntityManager.AddComponent<ClientReady>(rpc);

#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[_ClientLevelSyncSystem] Added ClientReeady to connection entity {connection}.");
#endif

            levelSyncState.State = LevelSyncState.Idle;
            SystemAPI.SetSingleton(levelSyncState);

#if UNITY_EDITOR
        var currentState = SystemAPI.GetSingleton<CurrentLevelSyncState>();
        UnityEngine.Debug.Log($"[_ClientLevelSyncSystem] Set level sync state ({currentState.State}) and current level equals {currentState.CurrentLevel} on {World.Name}.");
#endif

            // LoadingScreenUI.Set(LoadingScreenState.None);
        }
    }
}