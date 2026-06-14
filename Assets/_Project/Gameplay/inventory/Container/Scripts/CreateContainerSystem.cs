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
            var targetEntity = request.Entity;

            bool isWeaponEntity = SystemAPI.HasComponent<WithWeaponContainer>(targetEntity);
            bool isPlayerCharacterEntity = SystemAPI.HasComponent<WithCharacterContainers>(targetEntity);

            if (isPlayerCharacterEntity) 
            {
                var containers = SystemAPI.GetComponentRW<WithCharacterContainers>(targetEntity);
                var containersCapacity = SystemAPI.GetComponentRW<CharacterContainersCapacity>(targetEntity);

                int inventorySize = containersCapacity.ValueRW.BackpackSize;
                int weaponEquipmentSize = containersCapacity.ValueRW.WeaponEquipmentSize;
                int consumableEquipmentSize = containersCapacity.ValueRW.ConsumableEquipmentSize;

                Entity weaponEquipmentContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, targetEntity, 
                    ContainerType.CharacterWeaponEquipment, 
                    weaponEquipmentSize
                );

                Entity consumableEquipmentContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, targetEntity, 
                    ContainerType.CharacterConsumableEquipment, 
                    consumableEquipmentSize
                );

                Entity inventoryContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, targetEntity, 
                    ContainerType.CharacterInventory, 
                    inventorySize
                );
                
                ecb.SetComponent(targetEntity, new WithCharacterContainers
                {
                    InventoryContainer = inventoryContainer,                    
                    WeaponEquipmentContainer = weaponEquipmentContainer,
                    ConsumableEquipmentContainer = consumableEquipmentContainer,
                });

                ecb.AppendToBuffer(targetEntity, new LinkedEntityGroup 
                { 
                    Value = consumableEquipmentContainer 
                });

                ecb.AppendToBuffer(targetEntity, new LinkedEntityGroup 
                { 
                    Value = weaponEquipmentContainer 
                });

                ecb.AppendToBuffer(targetEntity, new LinkedEntityGroup 
                { 
                    Value = inventoryContainer 
                });

                // Add owner reference to container 

                AddOwnerEntityReferenceToContainer(ref ecb, targetEntity, inventoryContainer);
                AddOwnerEntityReferenceToContainer(ref ecb, targetEntity, consumableEquipmentContainer);
                AddOwnerEntityReferenceToContainer(ref ecb, targetEntity, weaponEquipmentContainer);
            }
            else if (isWeaponEntity)
            {
                var containerSize = SystemAPI.GetComponent<WeaponCapacity>(targetEntity).Value;

                Entity weaponContainer = InstantiateContainerAndSendInitRequest(
                    ref ecb, containerPrefab, targetEntity, ContainerType.Weapon, 
                    containerSize
                );

                ecb.SetComponent(targetEntity, new WithWeaponContainer
                {
                    Container = weaponContainer
                });

                ecb.AppendToBuffer(targetEntity, new LinkedEntityGroup 
                { 
                    Value = weaponContainer 
                });

                AddOwnerEntityReferenceToContainer(ref ecb, targetEntity, weaponContainer);
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

    private void AddOwnerEntityReferenceToContainer(
        ref EntityCommandBuffer ecb,
        Entity ownerEntity,
        Entity containerEntity 
    )
    {
        ecb.AddComponent(containerEntity, new ContainerOwnerEntityReference
        {
            Entity = ownerEntity
        });
    }
}