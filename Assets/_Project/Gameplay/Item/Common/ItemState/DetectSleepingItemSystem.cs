using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct DetectSleepingItemSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CustomItemPhysicsSettings>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var physicSettings = SystemAPI.GetSingleton<CustomItemPhysicsSettings>();

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new DetectSleepingItemJob
        {
            GroundedTimerLookup = SystemAPI.GetComponentLookup<GroundedTimer>(),
            DeltaTime = deltaTime,
            PhysicSettings = physicSettings,
            ECB = ecb
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[WithAll(typeof(GroundedPhysicsItemTag))]
[BurstCompile]
public partial struct DetectSleepingItemJob : IJobEntity
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public CustomItemPhysicsSettings PhysicSettings;

    [ReadOnly] public ComponentLookup<GroundedTimer> GroundedTimerLookup;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in PhysicsVelocity physicsVelocity,
        Entity entity
    )
    {
        // getting length of vectors
        float speedSq = math.lengthsq(physicsVelocity.Linear);
        float angularSq = math.lengthsq(physicsVelocity.Angular);

        bool isMoving = 
            speedSq > PhysicSettings.LinearThresholdSq ||
            angularSq > PhysicSettings.AngularThresholdSq;

        if (!GroundedTimerLookup.HasComponent(entity))
        {
            ECB.AddComponent(sortKey, entity, new GroundedTimer
            {
                Value = 0
            });
            
            return;
        }

        var timer = GroundedTimerLookup[entity];

        if (isMoving)
        {
            timer.Value = 0;
            ECB.SetComponent(sortKey, entity, timer);
        }
        else
        {
            timer.Value += DeltaTime;
            ECB.SetComponent(sortKey, entity, timer);

            if (timer.Value >= PhysicSettings.TimeToSleep)
            {
                ECB.RemoveComponent<PhysicsVelocity>(sortKey, entity);
                ECB.RemoveComponent<GroundedPhysicsItemTag>(sortKey, entity);
            }
        }
    }
}

public struct DroppedItemTag : IComponentData { } 

public struct GroundedTimer : IComponentData
{
    public float Value; 
}

public struct CustomItemPhysicsSettings : IComponentData
{
    public float TimeToSleep;
    public float LinearThresholdSq;
    public float AngularThresholdSq;
}