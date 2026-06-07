using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ApplyWeaponLevelSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var job = new ApplyWeaponLevelJob
        {
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),

            MaxManaLookup = SystemAPI.GetComponentLookup<WeaponMaxMana>(),
            SpreadLookup = SystemAPI.GetComponentLookup<WeaponSpread>(),
            IsShuffleLookup = SystemAPI.GetComponentLookup<WeaponShuffle>(),
            CastingSpellNumberLookup = SystemAPI.GetComponentLookup<WeaponCastSpellNumber>(),
            WeaponCastDelayLookup = SystemAPI.GetComponentLookup<WeaponCastDelay>(),
            ManaRegenerationSpeedLookup = SystemAPI.GetComponentLookup<WeaponManaRecoveryRate>(),
            WeaponCapacityLookup = SystemAPI.GetComponentLookup<WeaponCapacity>()
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
}

public partial struct ApplyWeaponLevelJob : IJobEntity
{
    public ComponentLookup<WeaponMaxMana> MaxManaLookup; 
    public ComponentLookup<WeaponSpread> SpreadLookup; 
    public ComponentLookup<WeaponShuffle> IsShuffleLookup;
    public ComponentLookup<WeaponCastSpellNumber> CastingSpellNumberLookup;
    public ComponentLookup<WeaponCastDelay> WeaponCastDelayLookup;
    public ComponentLookup<WeaponManaRecoveryRate> ManaRegenerationSpeedLookup;
    public ComponentLookup<WeaponCapacity> WeaponCapacityLookup;

    public EntityCommandBuffer ECB;

    public void Execute(
        DynamicBuffer<ParamRaw> rawBuffer,
        CurrentWeaponLevel currentLevel,
        NeedApplyFinalValuesTag tag,
        Entity entity
    )
    {
        for (int i = 0; i < rawBuffer.Length; i++)
        {
            var e = rawBuffer[i];

            // Order is important (while)
            var levelFunctions = WeaponLevelFunctionsDatabase.Instance.Set.LevelFunctions[(int)currentLevel.Value];

            switch (e.Id)
            {
                case ParameterId.CastingSpells :
                    if (!CastingSpellNumberLookup.HasComponent(entity)) continue;
                    
                    ECB.SetComponent(entity, new WeaponCastSpellNumber 
                    { 
                        Value = levelFunctions.ApplyCastingSpells(e.Value)
                    });

                    break;

                case ParameterId.CastDelay :
                    if (!WeaponCastDelayLookup.HasComponent(entity)) continue;
                    
                    ECB.SetComponent(entity, new WeaponCastDelay 
                    { 
                        Value = levelFunctions.ApplyCastDelay(e.Value)
                    });

                    break;
                
                case ParameterId.MaxMana : 
                    if (!MaxManaLookup.HasComponent(entity)) 
                        continue;

                    ECB.SetComponent(entity, new WeaponMaxMana 
                    { 
                        Value = levelFunctions.ApplyMaxMana(e.Value)
                    });

                    break;
                
                case ParameterId.RegenerationManaSpeed : 
                    if (!ManaRegenerationSpeedLookup.HasComponent(entity)) continue;
                    
                    ECB.SetComponent(entity, new WeaponManaRecoveryRate 
                    { 
                        Value = levelFunctions.ApplyManaRecoveryRate(e.Value) 
                    });
                    
                    break;
                
                case ParameterId.Spread :
                    if (!SpreadLookup.HasComponent(entity)) continue;
                    
                    ECB.SetComponent(entity, new WeaponSpread 
                    { 
                        Value = levelFunctions.ApplySpread(e.Value) 
                    });
                    
                    break;

                case ParameterId.Shuffle : 
                    if (!IsShuffleLookup.HasComponent(entity)) continue;

                    ECB.SetComponent(entity, new WeaponShuffle 
                    { 
                        Value = levelFunctions.ApplyShuffle(e.Value) 
                    });
                    
                    break;

                case ParameterId.Capacity : 

                    ECB.SetComponent(entity, new WeaponCapacity 
                    { 
                        Value = levelFunctions.ApplyCapacity(e.Value) 
                    });
                    
                    break;
            }
        }

        ECB.SetComponentEnabled<NeedApplyFinalValuesTag>(entity, false);
        ECB.SetComponentEnabled<NeedToCreateContainer>(entity, true);
    }
}