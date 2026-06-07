using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.NetCode;

/// <summary>
/// Checks if character (having the interaction control component) has active interactValue 
/// and creates intentrion request. 
/// This system uses Physicss Raycast to define interactables 
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InteractSystem : ISystem
{
    private CollisionFilter _selectionFilter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {        
        _selectionFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5,     // RayCasts
            CollidesWith = 1 << 7, // Interactables  
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        
        var job = new PickupCollectableItemJob
        {
            CollisionWorld = collisionWorld,
            Filter = _selectionFilter,
            ECB = ecb,
        };
        
        job.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct PickupCollectableItemJob : IJobEntity
{
    [ReadOnly] public CollisionWorld CollisionWorld;
    [ReadOnly] public CollisionFilter Filter;

    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        Entity entity,
        in CharacterInteractionControl control,
        in CharacterInteractionDistance distance,
        in LocalTransform transform,
        in FirstPersonCharacterComponent character
    )
    {
        if (!control.Interact) return;

        float3 forwardLocal = math.forward(character.ViewLocalRotation);
        float3 forwardWorld = math.mul(transform.Rotation, forwardLocal);

        float3 start = transform.Position + new float3(0f, 0.4f, 0f);;
        float3 end = start + forwardWorld * distance.Value;

        RaycastInput selectionInput = new RaycastInput
        {
            Start = start,
            End = end,
            Filter = Filter,
        };

        if (CollisionWorld.CastRay(selectionInput, out var hit))
        {
            Entity requestEntity = ECB.CreateEntity(sortKey);

            ECB.AddComponent(sortKey, requestEntity, new InteractRequest
            {
                Interacter = entity,
                Interactable = hit.Entity
            });
        }
    }
}