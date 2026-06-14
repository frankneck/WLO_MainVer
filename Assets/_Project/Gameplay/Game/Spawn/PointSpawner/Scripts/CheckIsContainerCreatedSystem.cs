using UnityEngine;
using Unity.Entities;
using UnityEditor;
using Unity.NetCode;
using Unity.Burst;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(CreateCollectableItemBySpawnerSystem))]
[BurstCompile]
public partial struct CheckIsContainerCreatedSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (_, containerOwnerEntity) in SystemAPI
            .Query<NeedToCreateContainer>()
            .WithNone<EntityWithContainerTag>()
            .WithEntityAccess())
        {
            SendCreateContainerRequest(ref ecb, containerOwnerEntity);
        }   

        ecb.Playback(state.EntityManager);
    }

    private void SendCreateContainerRequest(
        ref EntityCommandBuffer ecb,
        Entity containerOwnerEntity)
    {
        Entity createCharacterContainersRequest = ecb.CreateEntity();
        ecb.AddComponent(createCharacterContainersRequest, new CreateContainerForEntityRequest
        {
            Entity = containerOwnerEntity,
        });

        ecb.SetComponentEnabled<NeedToCreateContainer>(containerOwnerEntity, false);
    }
}  