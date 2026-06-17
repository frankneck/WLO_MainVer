using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;

/// <summary>
/// Main logic of loading and unloading levels, also stores levels 
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class LevelLoaderSystem : SystemBase
{
    private NativeHashMap<int, EntitySceneReference> m_Levels;
    public NativeHashMap<int, EntitySceneReference> Levels => m_Levels;

    private int m_NativeHashMapSize;

    protected override void OnCreate()
    {
        RequireForUpdate<CurrentLevelSyncState>();
        RequireForUpdate<LevelListData>();
        m_Levels = new NativeHashMap<int, EntitySceneReference>(m_NativeHashMapSize, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        if (m_Levels.IsCreated)
            m_Levels.Dispose();
    }

    protected override void OnUpdate()
    {
        var levelList = SystemAPI.GetSingletonBuffer<LevelListData>();

        if (m_Levels.Count == 0)
        {
            m_NativeHashMapSize = levelList.Length;

            for (int i = 0; i < m_NativeHashMapSize; i++)
            {
                m_Levels.Add(levelList[i].LevelNumber, levelList[i].Scene);
            }

#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[_LevelLoaderSystem] NativeHashMap has been initialized. HashMap count equals {m_Levels.Count} on {World.Name}.");
#endif
            // Clear buffer because buffer is used once to load in this system
            levelList.Clear();
        }

        CheckLevelLoading();
    }

    /// <summary>
    /// Changes state scen loading if ther're loaded
    /// </summary>
    public void CheckLevelLoading()
    {
        var levelSyncState = SystemAPI.GetSingleton<CurrentLevelSyncState>();

        if (levelSyncState.State == LevelSyncState.LevelLoadInProgress)
        {
            var levelSyncStateEntity = SystemAPI.GetSingletonEntity<CurrentLevelSyncState>();
            var trackedSubScenes = SystemAPI.GetBuffer<TrackedSubscenes>(levelSyncStateEntity);
     
            bool allScenesLoaded = true;

            foreach (var sceneEntity in trackedSubScenes)
            {
                if (!SceneSystem.IsSceneLoaded(World.Unmanaged, sceneEntity.Entity))
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"[_LevelLoaderSystem] Not all scenes has loaded on {World.Name}.");
#endif

                    allScenesLoaded = false;
                }
            }

            if (allScenesLoaded)
            {

#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[_LevelLoaderSystem] All scenes has loaded on {World.Name}");
#endif         
                
                levelSyncState.State = LevelSyncState.LevelLoaded;
                SystemAPI.SetSingleton(levelSyncState);
            }    
        }
    }
    
    /// <summary>
    /// Loads new subscene and add to TrackedSubscenes
    /// </summary>
    public Entity LoadLevel(int number)
    {
        // Getting guid of scene
        var sceneRefs = m_Levels.GetValueArray(Allocator.Temp);
        var lookupScene = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneRefs[number]);
        
        // If subscene is already loaded continue 
        if (lookupScene != null && SceneSystem.IsSceneLoaded(World.Unmanaged, lookupScene))
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[_LoadLevel] Scene {lookupScene} by number {number} is already loaded on {World.Name}.");
#endif
            return Entity.Null;
        }

        var levelSyncState = SystemAPI.GetSingleton<CurrentLevelSyncState>();

        // Getting scene
        Entity sceneEntity = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneRefs[number]);
        
        var trackedSubscenes = SystemAPI.GetBuffer<TrackedSubscenes>(SystemAPI.GetSingletonEntity<CurrentLevelSyncState>());
        trackedSubscenes.Add(new TrackedSubscenes 
        { 
            Entity = sceneEntity 
        });

        // Change state
        levelSyncState.State = LevelSyncState.LevelLoadInProgress;
        levelSyncState.CurrentLevel = number;
        
        SystemAPI.SetSingleton(levelSyncState);

#if UNITY_EDITOR
        var currentState = SystemAPI.GetSingleton<CurrentLevelSyncState>();
        UnityEngine.Debug.Log($"[_LoadLevel] Set level sync state ({currentState.State}) and current level equals {currentState.CurrentLevel} on {World.Name}.");
#endif

        sceneRefs.Dispose();

        return sceneEntity;
    } 
} 