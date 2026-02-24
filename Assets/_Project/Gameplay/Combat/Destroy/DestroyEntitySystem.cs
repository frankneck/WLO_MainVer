using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged); 

        foreach (var (transform, entity) in SystemAPI
            .Query<RefRW<LocalTransform>>()
            .WithAll<Simulate, DestroyEntityTag>()
            .WithEntityAccess())
        {
            if (state.World.IsServer())
            {
                ecb.DestroyEntity(entity);
            }
            else
            {
                transform.ValueRW.Position = new float3(1000f, 1000f, 1000f);
            }
        }
    }
} 

