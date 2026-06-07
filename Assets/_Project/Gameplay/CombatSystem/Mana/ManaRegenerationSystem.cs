using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ApplyManaRegenSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (recoveryRate, mana, maxMana) in SystemAPI
            .Query<RefRO<WeaponManaRecoveryRate>, 
                RefRW<CurrentMana>, RefRO<WeaponMaxMana>>())
        {
            mana.ValueRW.Value += recoveryRate.ValueRO.Value * deltaTime;
            mana.ValueRW.Value = math.min(mana.ValueRW.Value, maxMana.ValueRO.Value);
        }
    }
}

