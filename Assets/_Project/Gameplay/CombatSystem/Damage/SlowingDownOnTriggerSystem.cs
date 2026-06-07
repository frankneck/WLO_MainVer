using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public partial struct SlowingDownOnTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (slowing, entity) in SystemAPI.Query<SlowingDown>().WithEntityAccess())
        {
            ecb.RemoveComponent<SlowingDown>(entity);
        }

        var jobHandle = new SlowingDownOnTriggerJob
        {
            JellyZoneLookup = SystemAPI.GetComponentLookup<JellyZone>(true),
            PlayerTagLookup = SystemAPI.GetComponentLookup<CharacterTag>(true),
            ECB = ecb
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = jobHandle.Schedule(simulationSingleton, state.Dependency);
    }
}

public struct SlowingDownOnTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<JellyZone> JellyZoneLookup;
    [ReadOnly] public ComponentLookup<CharacterTag> PlayerTagLookup;
    public EntityCommandBuffer ECB;

    public void Execute(TriggerEvent triggerEvent)
    {
        if (JellyZoneLookup.HasComponent(triggerEvent.EntityA) 
            && PlayerTagLookup.HasComponent(triggerEvent.EntityB))
        {
            if (JellyZoneLookup.TryGetComponent(triggerEvent.EntityA, out var jellyZone))
            {
                AddSlowingDownComponent(ref ECB, triggerEvent.EntityB, jellyZone);
            }
        }
        
        if (JellyZoneLookup.HasComponent(triggerEvent.EntityB) 
            && PlayerTagLookup.HasComponent(triggerEvent.EntityA))
        {
            if (JellyZoneLookup.TryGetComponent(triggerEvent.EntityB, out var jellyZone))
            {
                AddSlowingDownComponent(ref ECB, triggerEvent.EntityA, jellyZone);
            } 
        }
    }

    void AddSlowingDownComponent(ref EntityCommandBuffer ecb, Entity entity, JellyZone zone)
    {
        ECB.AddComponent( entity, new SlowingDown
        {
            SpeedMultiplier = zone.SpeedMultiplier,
            SharpnessMultiplier = zone.SharpnessMultiplier,
            AirAccelerationMultiplier = zone.AirAccelerationMultiplier,
            AirMaxSpeedMultiplier = zone.AirMaxSpeedMultiplier,
            AirDragMultiplier = zone.AirDragMultiplier,
            GravityMultiplier = zone.GravityMultiplier,
            JumpMultiplier = zone.JumpMultiplier
        });    
    }
}

public struct SlowingDown : IComponentData
{
    public float SpeedMultiplier;
    public float SharpnessMultiplier;
    public float AirAccelerationMultiplier;
    public float AirMaxSpeedMultiplier;
    public float AirDragMultiplier;
    public float GravityMultiplier;
    public float JumpMultiplier;
}