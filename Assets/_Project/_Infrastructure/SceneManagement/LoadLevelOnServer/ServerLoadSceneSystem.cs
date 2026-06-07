using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Receives request to load level on the server. Entry point
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(LevelLoaderSystem))]
public partial class ServerLoadSceneSystem : SystemBase
{
    private LevelLoaderSystem m_levelLoader;

    protected override void OnCreate()
    {
        m_levelLoader = World.GetExistingSystemManaged<LevelLoaderSystem>();
        RequireForUpdate<LevelSyncStateComponent>();
        RequireForUpdate<LevelListData>(); 
    }

    protected override void OnUpdate()
    {
        var shouldLoadNextLevel = false;
        var lvlNumber = -1;
        var levelSyncState = SystemAPI.GetSingleton<LevelSyncStateComponent>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (loadLevelRequest, entity) in SystemAPI
            .Query<LoadLevelRequest>()
            .WithEntityAccess())
        {
            shouldLoadNextLevel = true;
            lvlNumber = loadLevelRequest.LevelNumber;
            
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(EntityManager);
        ecb.Dispose();

        if (shouldLoadNextLevel && lvlNumber != -1)
        {
            // Change state of level
            levelSyncState.NextLevel = lvlNumber;
            levelSyncState.State = LevelSyncState.LevelLoadRequest;
            SystemAPI.SetSingleton(levelSyncState);

#if UNITY_EDITOR
            var currentState = SystemAPI.GetSingleton<LevelSyncStateComponent>();        
            UnityEngine.Debug.Log($"[_ServerLoadSceneSystem] Current level sync state {currentState.State} and next level {currentState.NextLevel}");
#endif

            m_levelLoader.LoadLevel(lvlNumber);
        }
    }
}