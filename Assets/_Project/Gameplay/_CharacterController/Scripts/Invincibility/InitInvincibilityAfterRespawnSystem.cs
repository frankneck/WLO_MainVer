using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct InitInvicibilityAfterRespawnSystem : ISystem
{
    [BurstCompile]
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

        foreach (var (timer, entity) in SystemAPI.Query<InvincibilityTimer>()
            .WithAll<InvincibilityTag>().WithEntityAccess().WithNone<InvincibilityEndAtTick>())
        {
            var lifeTimeInTicks = (uint) ( timer.Value * simulationTickRate );
            var targetTick = currentTick;
            
            // target is a tick when we need to destroy entity
            targetTick.Add(lifeTimeInTicks); 
            ecb.AddComponent(entity, new InvincibilityEndAtTick { Tick = targetTick });
        }     
        ecb.Playback(state.EntityManager);
    }
} 