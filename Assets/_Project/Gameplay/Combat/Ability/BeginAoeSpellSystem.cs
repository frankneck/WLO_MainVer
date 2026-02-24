using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct BeginAoeAbilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        foreach (var (transform, abilityPrefabs, team, input, abilityCooldownTargetTicks, cooldown) in SystemAPI
            .Query<LocalTransform, RefRO<SpellPrefabs>, GameTeam, AttackInput, 
                    DynamicBuffer<SpellCooldownTargetTicks>, SpellCooldown>()
            .WithAll<Simulate>())
        {


            var isOnCoolDown = true;
            var curAbilityTargetTick = new SpellCooldownTargetTicks();

            var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            var cooldownInTicks = (uint)(simulationTickRate * cooldown.AoeAbility);

            for (var i = 0u; i < networkTime.SimulationStepBatchSize; i++ )
            {
                var testTick = currentTick;
                testTick.Subtract(i);
                
                // if data is null
                if (!abilityCooldownTargetTicks.GetDataAtTick(testTick, out curAbilityTargetTick))
                {
                    curAbilityTargetTick.AoeAbility = NetworkTick.Invalid;
                }

                if (curAbilityTargetTick.AoeAbility == NetworkTick.Invalid ||
                    !curAbilityTargetTick.AoeAbility.IsNewerThan(currentTick))
                {
                    isOnCoolDown = false;
                    break;
                }
            }

            // if cooldown is active we are just waiting
            if (isOnCoolDown) continue;
            UnityEngine.Debug.Log("[BeginAoeSystem] Is On Cooldown");

            if (input.AoeAttack.IsSet)
            {
                UnityEngine.Debug.Log("[BeginAoeSystem] Attack");   
                var aoeEntity = ecb.Instantiate(abilityPrefabs.ValueRO.AoeAbility);
                var localTransform = transform;

                ecb.SetComponent(aoeEntity, localTransform);
                ecb.SetComponent(aoeEntity, team); 

                if (state.WorldUnmanaged.IsServer()) continue;
                var newCooldownTargetTick = currentTick;
                newCooldownTargetTick.Add(cooldownInTicks);
                curAbilityTargetTick.AoeAbility = newCooldownTargetTick;

                var nextTick = currentTick;
                nextTick.Add(1u);
                curAbilityTargetTick.Tick = nextTick;

                abilityCooldownTargetTicks.AddCommandData(curAbilityTargetTick);
            }
        }
    }
}
