using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ApplyDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();        
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;
        
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (health, damageTickBuffer, damageMultiplier, entity) in SystemAPI
            .Query<
                RefRW<CurrentHealth>, 
                DynamicBuffer<DamageThisTick>, 
                RefRO<DamageMultiplier>
                >()
            .WithAll<Simulate>()
            .WithNone<PendingDeathTag>()
            .WithEntityAccess())
        {
            if (!damageTickBuffer.GetDataAtTick(currentTick, out var damageThisTick)) 
                continue;
            
            if (damageThisTick.Tick != currentTick) 
                continue;

            health.ValueRW.Value -= damageThisTick.Value * damageMultiplier.ValueRO.Multiplier;
            
            // Destroy Entity
            if (health.ValueRO.Value <= 0)
            {        
                ecb.AddComponent<PendingDeathTag>(entity);
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose(); 
    }
}

public struct PendingDeathTag : IComponentData { }