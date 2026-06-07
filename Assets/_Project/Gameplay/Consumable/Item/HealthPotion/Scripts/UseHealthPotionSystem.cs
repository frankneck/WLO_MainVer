using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
// [BurstCompile]
public partial struct ReadUsingHealthPotionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        ReadUsingHealthPotionJob jobHandle = new ReadUsingHealthPotionJob
        {
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(HealthPotionTag))]
public partial struct ReadUsingHealthPotionJob : IJobEntity
{

    public EntityCommandBuffer ECB;  

    public void Execute(
        in HealthPotionVolume volume,   
        in ItemControl itemControl,
        ref EquipedBy character,
        Entity healthPotionItemEntity
    )
    {
        var characterEntity = character.Entity;

        if (itemControl.MainActionPressed)
        {
            var requestEntity = ECB.CreateEntity();
            ECB.AddComponent(requestEntity, new TryToUseHealthPotion
            {
                HealthPotionItemEntity = healthPotionItemEntity,
                CharacterEntity = characterEntity
            });
        } 
    }
}

public struct TryToUseHealthPotion : IComponentData
{
    public Entity HealthPotionItemEntity;
    public Entity CharacterEntity; 
}


public partial struct UseHealthPotionOnCharacterSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        UseHealthPotionOnCharacterJob jobHandle = new UseHealthPotionOnCharacterJob
        {
            MaxHealthLookup = SystemAPI.GetComponentLookup<MaxHealth>(true),
            HealthPotionVolumeLookup = SystemAPI.GetComponentLookup<HealthPotionVolume>(true),
            CurrentHealthLookup = SystemAPI.GetComponentLookup<CurrentHealth>(false),
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

public partial struct UseHealthPotionOnCharacterJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<MaxHealth> MaxHealthLookup;
    [ReadOnly] public ComponentLookup<HealthPotionVolume> HealthPotionVolumeLookup;
    public ComponentLookup<CurrentHealth> CurrentHealthLookup;

    public EntityCommandBuffer ECB;

    public void Execute(
        TryToUseHealthPotion request,
        Entity requestEntity
    )
    {
        var characterEntity = request.CharacterEntity;
        var item = request.HealthPotionItemEntity;

        if (!HealthPotionVolumeLookup.TryGetComponent(item, out HealthPotionVolume volume))
            return;

        if (!MaxHealthLookup.TryGetComponent(characterEntity, out MaxHealth maxHealth))
            return; 

        // Recovery health
        if (CurrentHealthLookup.HasComponent(characterEntity))
        {
            CurrentHealth characterHealth = CurrentHealthLookup[characterEntity];

            characterHealth.Value += volume.Value;

            characterHealth.Value = math.clamp(characterHealth.Value, 0, maxHealth.Value);

            CurrentHealthLookup[characterEntity] = characterHealth;

            // Send request to remove item from buffer
            Entity newRequestEntity = ECB.CreateEntity();
            ECB.AddComponent(newRequestEntity, new SpendConsumable
            {
                ConsumableItemEntity = item,
                CharacterEntity = characterEntity
            });
        }

        ECB.DestroyEntity(requestEntity);
    }
}