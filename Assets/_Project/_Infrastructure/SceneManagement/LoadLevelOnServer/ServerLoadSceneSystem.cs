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
        RequireForUpdate<CurrentLevelSyncState>();
        RequireForUpdate<LevelListData>(); 
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        CurrentLevelSyncState levelSyncState = SystemAPI.GetSingleton<CurrentLevelSyncState>();
        
        // Init values
        bool shouldLoadNextLevel = false;
        int lvlNumber = -1;
        Entity matchEntity = Entity.Null;
        
        foreach (var (loadLevelRequest, entity) in SystemAPI
            .Query<LoadLevelAndBindToMatch>()
            .WithEntityAccess())
        {
            shouldLoadNextLevel = true;
            lvlNumber = loadLevelRequest.LevelNumber;
            matchEntity = loadLevelRequest.MatchEntity;

            ecb.DestroyEntity(entity);
        }

        if (shouldLoadNextLevel && 
            lvlNumber != -1 &&
            matchEntity != Entity.Null
        )
        {
            // Change state of level
            levelSyncState.NextLevel = lvlNumber;
            levelSyncState.State = LevelSyncState.LevelLoadRequest;
            
            SystemAPI.SetSingleton(levelSyncState);

        #if UNITY_EDITOR
            var currentState = SystemAPI.GetSingleton<CurrentLevelSyncState>();        
            UnityEngine.Debug.Log($"[_ServerLoadSceneSystem] Current level sync state {currentState.State} and next level {currentState.NextLevel}");
        #endif
        
            // Getting loaded scene entity
            Entity sceneEntity = m_levelLoader.LoadLevel(lvlNumber);

            if (sceneEntity == Entity.Null)
                return;

            ecb.AddComponent(sceneEntity, new BelongsToMatch
            {
                Entity = matchEntity
            });
        }

        ecb.Playback(EntityManager);
    }
}