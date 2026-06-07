using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(DamageOnTriggerSystem))]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct DestroySpellOnTrigger : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var damageOnTriggerHandle = new DestroySpellOnTriggerJob
        {
            ProjectileMoveSpeedLookup = SystemAPI.GetComponentLookup<ProjectileMoveSpeed>(),
            ECB = ecb
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = damageOnTriggerHandle.Schedule(simulationSingleton, state.Dependency);
    }  
}

[BurstCompile]
public struct DestroySpellOnTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<ProjectileMoveSpeed> ProjectileMoveSpeedLookup;

    public EntityCommandBuffer ECB;

    public void Execute(TriggerEvent triggerEvent)
    {
        if (ProjectileMoveSpeedLookup.HasComponent(triggerEvent.EntityA))
        {
            ECB.AddComponent<DestroyEntityTag>(triggerEvent.EntityA);
        }

        if (ProjectileMoveSpeedLookup.HasComponent(triggerEvent.EntityB))
        {
            ECB.AddComponent<DestroyEntityTag>(triggerEvent.EntityB);
        }
    }
}