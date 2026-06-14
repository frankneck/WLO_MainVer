using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]
public partial struct RemoveDroppedItemFromBufferSystem : ISystem
{
    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ContainerBuffer>();
        state.RequireForUpdate<RemoveDroppedItemFromBuffer>();
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var job = new RemoveDroppedItemFromBufferJob
        {
            WeaponTagLookup = SystemAPI.GetComponentLookup<WeaponTag>(true),
            PlayerCharacterTagLookup = SystemAPI.GetComponentLookup<PlayerCharacterTag>(true),
            ContainerOwnerEntityReferenceLookup = SystemAPI.GetComponentLookup<ContainerOwnerEntityReference>(true),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
            ECB = ecb
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
}

// [BurstCompile]
public partial struct RemoveDroppedItemFromBufferJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<WeaponTag> WeaponTagLookup;
    [ReadOnly] public ComponentLookup<PlayerCharacterTag> PlayerCharacterTagLookup;

    [ReadOnly] public ComponentLookup<ContainerOwnerEntityReference> ContainerOwnerEntityReferenceLookup;
    public BufferLookup<ContainerBuffer> ContainerBufferLookup;
    
    public EntityCommandBuffer ECB;

    public void Execute(
        in RemoveDroppedItemFromBuffer request,
        Entity reqEntity)
    {
        var itemContainerEntity = request.ContainerEntity;

        // if container entity isn't valid skip it
        if (!ContainerBufferLookup.HasBuffer(request.ContainerEntity))
            return;

        // getting main data about drop event
        var containerBuffer = ContainerBufferLookup[request.ContainerEntity];
        int droppedIndex = request.IndexInBuffer;
        int requestedQuantity = request.ItemQuantity;

        Entity bufferItemEntity = containerBuffer[droppedIndex].ItemEntity;

        if (bufferItemEntity == Entity.Null)
            return;
        
        int bufferQuantityValue = containerBuffer[droppedIndex].Quantity;

        if (bufferQuantityValue == 0)
            return;

        // calculate remaining quantity
        int remainingQuanity = bufferQuantityValue - requestedQuantity;

        // get container's owner entity
        var containerOwner = ContainerOwnerEntityReferenceLookup[itemContainerEntity];

        // player character's container
        if (PlayerCharacterTagLookup.HasComponent(containerOwner.Entity))
        {
            UnityEngine.Debug.Log("Reset equipedby because character dropped item");

            // reset components: equiped by player character entity 
            ECB.SetComponent(bufferItemEntity, new EquipedBy 
            { 
                Entity = Entity.Null
            });
            
            // reset components: active item of player character entity
            if (remainingQuanity <= 0)
            {
                UnityEngine.Debug.Log($"Active item quantity equals {remainingQuanity} so active item is None");

                ECB.SetComponent(containerOwner.Entity, new ActiveItem 
                { 
                    Entity = Entity.Null
                });
            }
        }

        if (remainingQuanity <= 0)
        {
            bufferItemEntity = Entity.Null;
            remainingQuanity = 0;
        }

        // Update buffer container data 
        containerBuffer[droppedIndex] = new ContainerBuffer
        {
            ItemEntity = bufferItemEntity,
            Quantity = remainingQuanity
        };

        // Update container version 
        ContainerVersionHelper.UpdateContainerVersion(ECB, request.ContainerEntity);
        
        ECB.DestroyEntity(reqEntity);
    }
}

