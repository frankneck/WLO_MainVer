using Unity.NetCode;
using Unity.Entities;
using Unity.Entities.Serialization;

// Client
public struct SceneLoadRequest : IRpcCommand { }
public struct SceneLoadedRequest : IRpcCommand { }
public struct SceneLoading : IComponentData { }
public struct SceneLoaded : IComponentData { }
public struct ServerScenesReady : IComponentData { }

// Server
public struct ConfirmSceneLoadRequest : IRpcCommand { } 

// Buffer 
public struct EntitySceneReferenceBufferElementData : IBufferElementData
{
    public EntitySceneReference Scene;
}
