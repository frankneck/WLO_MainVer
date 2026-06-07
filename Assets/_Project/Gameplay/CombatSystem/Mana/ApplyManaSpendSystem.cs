using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// This System apply mana acumulated spend value for 1 Tick
/// </summary>
[BurstCompile]
public partial struct ApplyManaSpendSystem : ISystem
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
        var currentTick = networkTime.ServerTick;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (spellSpendThisTickBuffer, mana) in SystemAPI.Query<DynamicBuffer<ManaSpendThisTickBuffer>, RefRW<CurrentMana>>())
        {
            if (!spellSpendThisTickBuffer.GetDataAtTick(currentTick, out var manaSpendThisTick)) continue;
            if (currentTick != manaSpendThisTick.Tick) continue;

            var manaCost = manaSpendThisTick.Value;
            mana.ValueRW.Value -= manaCost;

            if (mana.ValueRW.Value <= 0)
                mana.ValueRW.Value = 0;
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }        
}