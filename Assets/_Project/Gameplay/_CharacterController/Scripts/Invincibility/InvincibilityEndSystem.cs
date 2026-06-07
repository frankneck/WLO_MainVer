using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct InvincibilityEndSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;
        
        foreach (var (destroyOnTime, entity) in SystemAPI.Query<InvincibilityEndAtTick>()
            .WithAll<InvincibilityTag>().WithEntityAccess())
        {
            if (currentTick.Equals(destroyOnTime.Tick) || currentTick.IsNewerThan(destroyOnTime.Tick))
            {
                ecb.RemoveComponent<InvincibilityTag>(entity);
                ecb.RemoveComponent<InvincibilityEndAtTick>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
} 