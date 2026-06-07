using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Marks player character that it is initialized. It needs to assign initialized character by player entity (through ControlledCharacter).
/// After player assigned for character, this character need to draw in color of team. 
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct PlayerCharacterInitializeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var jobHandle = new PlayerCharacterInitializeJob
        {
            CharacterOwnerLookup = SystemAPI.GetComponentLookup<CharacterOwner>(true),
            PlayerCharacterInitialized = SystemAPI.GetComponentLookup<PlayerCharacterInitializedTag>(true),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(true),
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct PlayerCharacterInitializeJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CharacterOwner> CharacterOwnerLookup;
    [ReadOnly] public ComponentLookup<PlayerCharacterInitializedTag> PlayerCharacterInitialized;

    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    public EntityCommandBuffer ECB;

    public void Execute(
        in WithCharacterContainers containers,
        in EntityWithContainerTag entityWithContainerTag,
        in PlayerCharacterTag playerCharacterTag,
        Entity characterEntity
    )
    {
        if (PlayerCharacterInitialized.HasComponent(characterEntity))
        {
            // Player is already initialized
            return;
        }

        if (containers.ConsumableEquipmentContainer == null 
            || containers.WeaponEquipmentContainer == null 
            || containers.InventoryContainer == null)
        {
            // Player character doesn't have containers
            return;
        }

        var consumableEquipBuffer = ContainerBufferLookup[containers.ConsumableEquipmentContainer];
        var weaponEquipBuffer = ContainerBufferLookup[containers.WeaponEquipmentContainer];
        var inventoryBuffer = ContainerBufferLookup[containers.InventoryContainer];

        if (consumableEquipBuffer.Length == 0 ||
            weaponEquipBuffer.Length == 0 ||
            inventoryBuffer.Length == 0)
        {
            // Player character has containers but they don't have initialized
            return;
        }

        var playerEntity = CharacterOwnerLookup[characterEntity].Entity;

        var needToInitPlayerRequest = ECB.CreateEntity();
        ECB.AddComponent(needToInitPlayerRequest, new AssignCharacterToPlayer
        {
            PlayerEntity = playerEntity,
            CharacterEntity = characterEntity
        });

        ECB.AddComponent<PlayerCharacterInitializedTag>(characterEntity);
        
        ECB.AddComponent<NeedToInitEquipmentTag>(characterEntity);
    }
}