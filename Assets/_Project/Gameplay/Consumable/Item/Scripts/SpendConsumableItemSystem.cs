using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
// [BurstCompile]
public partial struct SpendConsumableItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        SpendConsumableItemJob jobHandle = new SpendConsumableItemJob
        {
            WithCharacterContainersLookup = SystemAPI.GetComponentLookup<WithCharacterContainers>(true),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(false),
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

// [BurstCompile]
public partial struct SpendConsumableItemJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<WithCharacterContainers> WithCharacterContainersLookup;

    public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    public EntityCommandBuffer ECB;  

    public void Execute(
        SpendConsumable request,
        Entity entity
    )
    {
        var characterEntity = request.CharacterEntity;
        var itemEntity = request.ConsumableItemEntity;

        if (WithCharacterContainersLookup.HasComponent(characterEntity))
        {
            Entity consumableContainer = WithCharacterContainersLookup[characterEntity]
                .ConsumableEquipmentContainer;

            DynamicBuffer<ContainerBuffer> buffer = ContainerBufferLookup[consumableContainer];

            for (int i = 0; i < buffer.Length; i++)
            {
                if (itemEntity != buffer[i].ItemEntity)
                    continue;

                var count = buffer[i].Quantity;

                if (count > 1)
                {
                    buffer[i] = new ContainerBuffer
                    {
                        ItemEntity = itemEntity,
                        Quantity = count - 1 
                    };
                }
                else
                {
                    UnityEngine.Debug.Log("[SpendConsumableItemJob]");

                    buffer[i] = new ContainerBuffer
                    {
                        ItemEntity = Entity.Null,
                        Quantity = 0 
                    };
                }
                UnityEngine.Debug.Log($"[SpendConsumableItemJob] the entity {itemEntity} add destory tag");

                ECB.AddComponent<DestroyEntityTag>(itemEntity);

                UpdateContainerVersion(ref ECB, consumableContainer);
            }
        }

        ECB.DestroyEntity(entity);
    }

    private void UpdateContainerVersion(
        ref EntityCommandBuffer ecb, 
        Entity container)
    {
        var request = ecb.CreateEntity();
        
        ecb.AddComponent(request, new UpdateContainerVersion 
        { 
            Container = container 
        });
    }
}