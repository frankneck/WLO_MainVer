using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct DestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        foreach (var (destroyAtTick , entity) in SystemAPI
            .Query<DestroyAtTick>().WithAll<Simulate>()
            .WithNone<DestroyEntityTag>().WithEntityAccess())
        {
            if ( currentTick.Equals(destroyAtTick.Value) || currentTick.IsNewerThan(destroyAtTick.Value) )
            {
                ecb.AddComponent(entity, new DestroyEntityTag());
            }
        }
    }
}