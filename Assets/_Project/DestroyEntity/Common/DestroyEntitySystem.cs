using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

/// <summary>
/// Entry point destroy system.
/// Destroy entity only here to avoid race conditions.  
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
[BurstCompile]
public partial struct DestroyEntitySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick)
            return;

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        foreach (var (localTransform, entity) in SystemAPI
            .Query<RefRW<LocalTransform>>()
            .WithAll<DestroyEntityTag, Simulate>()
            .WithEntityAccess())
        {            
            ecb.DestroyEntity(entity);
        }
    }
}