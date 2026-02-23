using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.NetCode;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

// Server must confirm request from the client
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class ServerHandleSceneLoadingSystem : SystemBase
{
    private EntityQuery _newRequests;

    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<SceneLoadRequest, ReceiveRpcCommandRequest>();
        _newRequests = GetEntityQuery(builder);
        RequireForUpdate(_newRequests);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (sceneLoadRequest, receiveRPC, entity) in SystemAPI.Query<SceneLoadRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            var confirmSceneLoadRequest = ecb.CreateEntity();
            ecb.AddComponent(confirmSceneLoadRequest, new ConfirmSceneLoadRequest());
            ecb.AddComponent(confirmSceneLoadRequest, new SendRpcCommandRequest { TargetConnection = receiveRPC.SourceConnection });

            ecb.DestroyEntity(entity);
            
            Debug.Log("[Server] Server confirmed scene load request on client");
        }
        
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

// Server accept client loaded scene 
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class LoadedSceneHandlingOnServer : SystemBase
{
    private EntityQuery _newRequests;

    protected override void OnCreate()
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<SceneLoadedRequest, ReceiveRpcCommandRequest>();
        _newRequests = GetEntityQuery(builder);
        RequireForUpdate(_newRequests);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (sceneLoadRequest, receiveRPC, entity) in SystemAPI.Query<SceneLoadedRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log("[Server] Server accepted client scene status (loaded)");
            ecb.AddComponent<ReadySpawn>(receiveRPC.SourceConnection);
            
            ecb.DestroyEntity(entity);
        }
        
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

// Server load scene
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
public partial class SceneLoaderOnServerSystem : SystemBase
{
    private EntityQuery newRequests;
    private LevelConfig _levelConfig;

    protected override void OnCreate()
    {
        newRequests = GetEntityQuery(typeof(EntitySceneReferenceBufferElementData));
        _levelConfig = Resources.Load<LevelConfig>("LevelConfig");
    }

    protected override void OnUpdate()
    {
        // Scene load
        var buffer = SystemAPI.GetSingletonBuffer<EntitySceneReferenceBufferElementData>();
        
        SceneLoaderService.LoadScenes(World, buffer, _levelConfig);
        
        Debug.Log("[Server] Scene loaded on server");

        Enabled = false;
    }
}

public static class SceneLoaderService
{
    public static void LoadScenes(World world, DynamicBuffer<EntitySceneReferenceBufferElementData> buffer, LevelConfig levelConfig)
    {
        foreach (var scene in levelConfig.Scenes)
        {
            buffer.Add(new EntitySceneReferenceBufferElementData
            {
                Scene = scene
            });

        }
        
        // replication of data (cause this structural changes)
        var scenes = new NativeArray<EntitySceneReference>(buffer.Length, Allocator.Temp);
        for (int i = 0; i < buffer.Length; i++)
        {
            scenes[i] = buffer[i].Scene;
        }

        foreach (var scene in scenes)
        {
            SceneSystem.LoadSceneAsync(world.Unmanaged, scene);
        }

        scenes.Dispose();
    }
}