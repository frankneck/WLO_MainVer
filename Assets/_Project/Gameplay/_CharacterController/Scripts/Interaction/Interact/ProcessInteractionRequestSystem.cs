using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Proccess intention (control) from character to interact   
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ProcessInteractionRequestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new ProcessInteractionRequestJob
        { 
            CollectableItemLookup = SystemAPI.GetComponentLookup<CurrentPickupMode>(true),
            ECB = ecb,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct ProcessInteractionRequestJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CurrentPickupMode> CollectableItemLookup;
    
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        ref InteractRequest request,
        Entity entity
    )
    {
        // Collectables Moves to Inventory

        if (CollectableItemLookup.TryGetComponent(request.Interactable, out var collectableItem))
        {   
            if (collectableItem.Value != PickupMode.OnInteract)
                return;

            var requestEntity = ECB.CreateEntity(sortKey);

            ECB.AddComponent(sortKey, requestEntity, new AddToInventoryRequest
            {
                Collector = request.Interacter,
                Collectable = request.Interactable
            });
        }

        ECB.DestroyEntity(sortKey, entity);
    }
} 