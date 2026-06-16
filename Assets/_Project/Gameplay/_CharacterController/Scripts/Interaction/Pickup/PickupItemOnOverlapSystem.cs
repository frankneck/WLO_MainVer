using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]

public partial struct PickupOnOverlapSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var jobHandle = new PickupOnOverlapJob
        {
            ECB = ecb,
            CharacterLookup = SystemAPI.GetComponentLookup<WithCharacterContainers>(true),
            CollectableItemLookup = SystemAPI.GetComponentLookup<CurrentPickupMode>(true)
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = jobHandle.Schedule(simulationSingleton, state.Dependency);
    }
}

// [BurstCompile]
public struct PickupOnOverlapJob : ITriggerEventsJob
{
    public EntityCommandBuffer ECB;
    
    [ReadOnly] public ComponentLookup<WithCharacterContainers> CharacterLookup;
    [ReadOnly] public ComponentLookup<CurrentPickupMode> CollectableItemLookup;

    public void Execute(TriggerEvent triggerEvent)
    {
        var entityA = triggerEvent.EntityA;
        var entityB = triggerEvent.EntityB;

        bool aIsCharacter = CharacterLookup.HasComponent(entityA);
        bool bIsCharacter = CharacterLookup.HasComponent(entityB);

        bool aIsCollectable = CollectableItemLookup.HasComponent(entityA);
        bool bIsCollectable = CollectableItemLookup.HasComponent(entityB);

        // Define roles
        Entity character;
        Entity collectable;

        if (aIsCharacter && bIsCollectable)
        {
            character = entityA;
            collectable = entityB;
        }
        else if (bIsCharacter && aIsCollectable)
        {
            character = entityB;
            collectable = entityA;
        }
        else
        {
            return;
        }

        var collectableMode = CollectableItemLookup[collectable].Value;
        if ((collectableMode & PickupMode.OnOverlap) != 0)
        {
            UnityEngine.Debug.Log($"[PickupOnOverlapJob] Add to inventory item request has been created for item {collectable}");

            // Create request to add to character inventory
            var request = ECB.CreateEntity();
            ECB.AddComponent(request, new AddToInventoryRequest
            {
                Collector = character,
                Collectable = collectable
            });
        }
    }
}