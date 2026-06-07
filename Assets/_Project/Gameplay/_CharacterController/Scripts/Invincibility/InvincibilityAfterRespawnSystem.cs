using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct InvincibilityAfterRespawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DamageMultiplier>();
        state.RequireForUpdate<InvincibilityTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {        
        foreach (var multiplier in SystemAPI.Query<RefRW<DamageMultiplier>>().WithAll<Simulate, InvincibilityTag>())
        {
            multiplier.ValueRW.Multiplier = 0;
        }
    }
} 

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct RemoveInvincibilitySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DamageMultiplier>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var modifier in SystemAPI.Query<RefRW<DamageMultiplier>>().WithNone<InvincibilityTag>())
        {
            modifier.ValueRW.Multiplier = 1f;
        }
    }
}