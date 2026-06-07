using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Creates container for entity from request
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct CreateContainerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GhostPrefabs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var containerPrefab = SystemAPI.GetSingleton<GhostPrefabs>().ItemContainer;

        foreach (var (request, entity) in SystemAPI
            .Query<CreateContainerForEntityRequest>()
            .WithEntityAccess())
        {
            var containerTargetEntity = request.Entity;

            bool isWeaponEntity = SystemAPI.HasComponent<WithWeaponContainer>(containerTargetEntity);
            bool isPlayerCharacterEntity = SystemAPI.HasComponent<WithCharacterContainers>(containerTargetEntity);

            if (isPlayerCharacterEntity) 
            {
                var containers = SystemAPI.GetComponentRW<WithCharacterContainers>(containerTargetEntity);
                var containersCapacity = SystemAPI.GetComponentRW<CharacterContainersCapacity>(containerTargetEntity);

                int inventorySize = containersCapacity.ValueRW.BackpackSize;
                int weaponEquipmentSize = containersCapacity.ValueRW.WeaponEquipmentSize;
                int consumableEquipmentSize = containersCapacity.ValueRW.ConsumableEquipmentSize;

                Entity weaponEquipmentContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, containerTargetEntity, 
                    ContainerType.CharacterWeaponEquipment, 
                    weaponEquipmentSize
                );

                Entity consumableEquipmentContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, containerTargetEntity, 
                    ContainerType.CharacterConsumableEquipment, 
                    consumableEquipmentSize
                );

                Entity inventoryContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, containerTargetEntity, 
                    ContainerType.CharacterInventory, 
                    inventorySize
                );
                
                ecb.SetComponent(containerTargetEntity, new WithCharacterContainers
                {
                    InventoryContainer = inventoryContainer,                    
                    WeaponEquipmentContainer = weaponEquipmentContainer,
                    ConsumableEquipmentContainer = consumableEquipmentContainer,
                });

                ecb.AppendToBuffer(containerTargetEntity, new LinkedEntityGroup 
                { 
                    Value = consumableEquipmentContainer 
                });

                ecb.AppendToBuffer(containerTargetEntity, new LinkedEntityGroup 
                { 
                    Value = weaponEquipmentContainer 
                });

                ecb.AppendToBuffer(containerTargetEntity, new LinkedEntityGroup 
                { 
                    Value = inventoryContainer 
                });

            }
            else if (isWeaponEntity)
            {
                var containerSize = SystemAPI.GetComponent<WeaponCapacity>(containerTargetEntity).Value;

                Entity weaponContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, containerTargetEntity, ContainerType.Weapon, 
                    containerSize
                );

                ecb.SetComponent(containerTargetEntity, new WithWeaponContainer
                {
                    Container = weaponContainer
                });

                ecb.AppendToBuffer(containerTargetEntity, new LinkedEntityGroup 
                { 
                    Value = weaponContainer 
                });
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }

    private Entity InstantiateContainerAndSendInitRequest(
        ref EntityCommandBuffer ecb,
        Entity containerPrefabEntity,
        Entity containerTargetEntity,
        ContainerType containerType,
        int containerSize)
    {
        var container = ecb.Instantiate(containerPrefabEntity);

        ecb.AddComponent(container, new InitContainerRequest
        {
            Item = containerTargetEntity,
            ItemContainer = container,
            Type = containerType,
            Size = containerSize  
        });

        ecb.AddComponent<EntityWithContainerTag>(containerTargetEntity);

        return container;
    }
}