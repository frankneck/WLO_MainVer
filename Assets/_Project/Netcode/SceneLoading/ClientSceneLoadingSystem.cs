#if !UNITY_SERVER
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;
using UnityEngine;

// System do first request to server
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class SceneLoadSendingOnClientSystem : SystemBase
{
    private EntityQuery _networkIDsQuery;

    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
        _networkIDsQuery = GetEntityQuery(builder);
        RequireForUpdate(_networkIDsQuery);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var enitties = _networkIDsQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in enitties)
        {
            var sceneLoadRequest = ecb.CreateEntity();
            ecb.AddComponent(sceneLoadRequest, new SceneLoadRequest());
            ecb.AddComponent(sceneLoadRequest, new SendRpcCommandRequest { TargetConnection = entity });

            Debug.Log("[Client] Scene load request sent");
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

// System do second request to accept loading
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class SceneLoadingOnClientSystem : SystemBase
{
    private EntityQuery _newReceiveRequests;

    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<ConfirmSceneLoadRequest, ReceiveRpcCommandRequest>();
        _newReceiveRequests = GetEntityQuery(builder);

        RequireForUpdate(_newReceiveRequests);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (sceneLoadRequest, receiveRPC, entity) in SystemAPI.Query<ConfirmSceneLoadRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
        {   
            
            // create new trigger entity for loading scene system 
            var sceneReadyToLoading = ecb.CreateEntity();
            ecb.AddComponent(sceneReadyToLoading, new SceneLoading());

            ecb.DestroyEntity(entity);
            
            Debug.Log("[Client] Server confirmation for scene loading received");
        }
        
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class SceneLoaderOnClientSystem : SystemBase
{
    private EntityQuery _bufferQuery;
    private LevelConfig _levelConfig;

    protected override void OnCreate()
    {        
        _bufferQuery = GetEntityQuery(typeof(EntitySceneReferenceBufferElementData));
        _levelConfig = Resources.Load<LevelConfig>("LevelConfig");
        
        RequireForUpdate<SceneLoading>();
        RequireForUpdate(_bufferQuery);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
    
        // Scene load
        var buffer = SystemAPI.GetSingletonBuffer<EntitySceneReferenceBufferElementData>();
        SceneLoaderService.LoadScenes(World, buffer, _levelConfig);

        ecb.Playback(EntityManager);
        ecb.Dispose();

        Debug.Log("[Client] Scene loading started");

        Enabled = false;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ServerSceneLoadMonitorSystem : SystemBase
{
    private EntityQuery _newRequests;
    private EntityQuery _sceneLoadingQuery;
    private bool _scenesReady;
    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<SceneReference>();
        _newRequests = GetEntityQuery(builder);
        _sceneLoadingQuery = GetEntityQuery(typeof(SceneLoading));
        
        RequireForUpdate(_newRequests);
        RequireForUpdate(_sceneLoadingQuery);
    }
    protected override void OnUpdate()
    {
        if (_scenesReady) return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var sceneRefs = _newRequests.ToEntityArray(Allocator.Temp);
        if (sceneRefs.Length == 0) return;

        bool allLoaded = true;
        
        foreach (var scene in sceneRefs)
        {
            if (!SceneSystem.IsSceneLoaded(World.Unmanaged, scene))
            {
                allLoaded = false;
                break;
            }
        }

        if (allLoaded)
        {
            _scenesReady = true;

            var sceneLoadings = _sceneLoadingQuery.ToEntityArray(Allocator.Temp);

            foreach (var sceneLoading in sceneLoadings)
            {
                ecb.AddComponent<SceneLoaded>(sceneLoading);
                ecb.RemoveComponent<SceneLoading>(sceneLoading);
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class SceneLoadedOnClientSystem : SystemBase
{
    private EntityQuery _sceneLoadedQuery;

    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<SceneLoaded>();
        _sceneLoadedQuery = GetEntityQuery(builder);
        
        RequireForUpdate(_sceneLoadedQuery);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (netId, entity) in SystemAPI.Query<NetworkId>().WithEntityAccess())
        {
            var sceneLoadedRequest = ecb.CreateEntity();
            ecb.AddComponent(sceneLoadedRequest, new SceneLoadedRequest());
            ecb.AddComponent(sceneLoadedRequest, new SendRpcCommandRequest { TargetConnection = entity });
            
            Debug.Log("[Client] Notification sent to server that scene has been fully loaded");
        }

        var sceneLoadeds = _sceneLoadedQuery.ToEntityArray(Allocator.Temp);
        foreach(var sceneLoaded in sceneLoadeds)
        {
            ecb.RemoveComponent<SceneLoaded>(sceneLoaded);
        }

        ecb.Playback(EntityManager);
        
        // Turn off loading screen
        LoadingScreenUI.Set(LoadingScreenState.None);
    }
}
#endif