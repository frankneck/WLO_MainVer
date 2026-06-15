using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
public partial struct DamageOnTriggerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var damageOnTriggerHandle = new DamageOnTriggerJob
        {
            DamageOnTriggerLookup = SystemAPI.GetComponentLookup<DamageOnTrigger>(true),
            GameTeamLookup = SystemAPI.GetComponentLookup<GameTeam>(true),
            AlreadyDamagedLookup = SystemAPI.GetBufferLookup<AlreadyDamagedEntity>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageBufferElement>(true),
            ProjectileOwnerLookup = SystemAPI.GetComponentLookup<ProjectileCasterEntityReference>(true),
            ECB = ecb,
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = damageOnTriggerHandle.Schedule(simulationSingleton, state.Dependency);
    }    
}

public struct DamageOnTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<DamageOnTrigger> DamageOnTriggerLookup; 
    [ReadOnly] public ComponentLookup<GameTeam> GameTeamLookup;
    [ReadOnly] public ComponentLookup<ProjectileCasterEntityReference> ProjectileOwnerLookup;
    [ReadOnly] public BufferLookup<AlreadyDamagedEntity> AlreadyDamagedLookup;
    [ReadOnly] public BufferLookup<DamageBufferElement> DamageBufferLookup;

    public EntityCommandBuffer ECB;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity damageDealingEntity;
        Entity damageReceivingEntity;
        
        if (DamageOnTriggerLookup.HasComponent(triggerEvent.EntityA) &&
            DamageBufferLookup.HasBuffer(triggerEvent.EntityB))
        {
            damageDealingEntity = triggerEvent.EntityA;
            damageReceivingEntity = triggerEvent.EntityB;
        }
        else if (DamageOnTriggerLookup.HasComponent(triggerEvent.EntityB) &&
            DamageBufferLookup.HasBuffer(triggerEvent.EntityA))
        {
            damageDealingEntity = triggerEvent.EntityB;
            damageReceivingEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        // Don't use damage multiple times
        var alreadyDamagedBuffer = AlreadyDamagedLookup[damageDealingEntity];
        foreach (var alreadyDamagedEntity in alreadyDamagedBuffer)
        {
            if (alreadyDamagedEntity.Value.Equals(damageReceivingEntity)) return;
        } 

        // Ignore friendly fire
        if (GameTeamLookup.TryGetComponent(damageDealingEntity, out var dealingTeam)
            && GameTeamLookup.TryGetComponent(damageReceivingEntity, out var receivingTeam))
        {   
            // if it's friendly fire -> skip but if it is None it means the entities hasn't Team
            if (dealingTeam.Value == receivingTeam.Value && 
                dealingTeam.Value != TeamType.None)
            {
                return;
            }
        }
        
        // if damaging entity is spell projectile 
        if (ProjectileOwnerLookup.HasComponent(damageDealingEntity))
        {
            Entity projectileOwnerEntity = ProjectileOwnerLookup[damageDealingEntity].Entity;

            ECB.AddComponent(damageReceivingEntity, new LastDamager 
            { 
                Entity = projectileOwnerEntity,
            });
        }

        var damageOnTrigger = DamageOnTriggerLookup[damageDealingEntity];

        ECB.AppendToBuffer(damageDealingEntity, new AlreadyDamagedEntity 
        { 
            Value = damageReceivingEntity 
        });

        ECB.AppendToBuffer(damageReceivingEntity, new DamageBufferElement 
        { 
            Value = damageOnTrigger.Value 
        });
    }
}

