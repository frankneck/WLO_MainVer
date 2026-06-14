using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct DefineSleepingItemSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new DefineSleepingItemJob
        {
            ItemLookup = SystemAPI.GetComponentLookup<ItemTag>(true),
            SleepingPhysicItemLookup = SystemAPI.GetComponentLookup<GroundedPhysicsItemTag>(true),
            PhysicsColliderLookup = SystemAPI.GetComponentLookup<PhysicsCollider>(true),
            ECB = ecb,
        };

        state.Dependency = job.Schedule(simulationSingleton, state.Dependency);
    }
}

[WithNone(typeof(GroundedPhysicsItemTag))]
[WithAll(typeof(DroppedItemTag))]
[BurstCompile]
public struct DefineSleepingItemJob : ICollisionEventsJob
{
    private const uint GroundPlaneCategory = 1u << 0; // ground plane category name
    private const uint StructuresCategory = 1u << 4; // structure category name

    [ReadOnly] public ComponentLookup<ItemTag> ItemLookup;
    [ReadOnly] public ComponentLookup<GroundedPhysicsItemTag> SleepingPhysicItemLookup;
    [ReadOnly] public ComponentLookup<PhysicsCollider> PhysicsColliderLookup;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(CollisionEvent collisionEvent)
    {
        var entityA = collisionEvent.EntityA;
        var entityB = collisionEvent.EntityB;

        bool aIsItem = ItemLookup.HasComponent(entityA);
        bool bIsItem = ItemLookup.HasComponent(entityB);

        if (aIsItem == bIsItem)
        {
            return;
        }

        if (aIsItem && !SleepingPhysicItemLookup.HasComponent(entityA) && IsRelevantCollisionPartner(entityB))
        {
            ECB.AddComponent<GroundedPhysicsItemTag>(entityA.Index, entityA);
        }
        else if (bIsItem && !SleepingPhysicItemLookup.HasComponent(entityB) && IsRelevantCollisionPartner(entityA))
        {
            ECB.AddComponent<GroundedPhysicsItemTag>(entityB.Index, entityB);
        }
    }

    private bool IsRelevantCollisionPartner(Entity entity)
    {
        if (!PhysicsColliderLookup.HasComponent(entity))
        {
            return false;
        }

        var physicsCollider = PhysicsColliderLookup[entity];
        if (!physicsCollider.Value.IsCreated)
        {
            return false;
        }

        CollisionFilter filter = physicsCollider.Value.Value.GetCollisionFilter();
        bool relevant = (filter.BelongsTo & (GroundPlaneCategory | StructuresCategory)) != 0;
        return relevant;
    }
}

public struct GroundedPhysicsItemTag : IComponentData { }