using Unity.Entities;
using UnityEngine;

/// <summary>
/// Help to avoid duplication of creation method 
/// </summary>
public static class PlayerStateHelper
{
    public static void SendUpdateCurrentPlayerStateRequest(
        ref EntityCommandBuffer ecb,
        Entity playerEntity,
        PlayerState newState
    )
    {
        var request = ecb.CreateEntity();
        ecb.AddComponent(request, new UpdateCurrentPlayerState
        {
            PlayerEntity = playerEntity,
            NewState = newState
        });
    }
} 

public static class RegisterDocumentHelper
{
    public static Entity RegisterDocument(
        ref EntityManager em,
        MonoBehaviour controller 
    )
    {
        var entity = em.CreateEntity();
        em.AddComponentObject(entity, controller);

        return entity;
    }
}

public static class ContainerVersionHelper
{
    public static void UpdateContainerVersion(
        EntityCommandBuffer ecb,
        Entity container
    )
    {
        var request = ecb.CreateEntity();
        
        ecb.AddComponent(request, new UpdateContainerVersion 
        { 
            Container = container 
        });
    }
}