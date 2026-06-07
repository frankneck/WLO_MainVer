using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public partial struct InitializationDestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state) 
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        if (!currentTick.IsValid)
            return;

        foreach (var (destroyOnTimer, entity) in SystemAPI
            .Query<DestroyOnTimer>()
            .WithEntityAccess()
            .WithNone<DestroyAtTick>())
        {
            var lifeTimeInTicks = (uint) ( destroyOnTimer.Value * simulationTickRate );
            var targetTick = currentTick;
            
            // target is a tick when we need to destroy entity
            targetTick.Add(lifeTimeInTicks); 
            ecb.AddComponent(entity, new DestroyAtTick { Value = targetTick });
        }     
        ecb.Playback(state.EntityManager);
    }
} 