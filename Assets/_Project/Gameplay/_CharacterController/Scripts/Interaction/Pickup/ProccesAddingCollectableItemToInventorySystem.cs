using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;

/// <summary>
/// Proccess request to add collectable the receiving entity in inventory buffer if the dealing entity has    
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ProccesAddingCollectableItemToInventorySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ItemDataBlobArray>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var itemDB = SystemAPI.GetSingleton<ItemDataBlobArray>();

        var job = new ProccesAddingCollectableItemToInventoryJob
        {
            CurrentItemIdLookup = SystemAPI.GetComponentLookup<CurrentItemId>(true),
            CharacterContainersLookup = SystemAPI.GetComponentLookup<WithCharacterContainers>(true),
            SpawnedByLookup = SystemAPI.GetComponentLookup<SpawnedBy>(true),
            itemsData = itemDB,

            CurrentSpawnerStateLookup = SystemAPI.GetComponentLookup<CurrentSpawnerState>(),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
            
            ECB = ecb,
        }; 

        state.Dependency = job.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct ProccesAddingCollectableItemToInventoryJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CurrentItemId> CurrentItemIdLookup;
    [ReadOnly] public ComponentLookup<WithCharacterContainers> CharacterContainersLookup;
    [ReadOnly] public ComponentLookup<SpawnedBy> SpawnedByLookup;
    [ReadOnly] public ItemDataBlobArray itemsData;

    public ComponentLookup<CurrentSpawnerState> CurrentSpawnerStateLookup;
    
    public BufferLookup<ContainerBuffer> ContainerBufferLookup;
    
    public EntityCommandBuffer ECB;

    public void Execute(
        ref AddToInventoryRequest request,
        Entity requestEntity)
    {
        TryAddToInventory(itemsData, request.Collector, request.Collectable);
        ECB.DestroyEntity(requestEntity);
    }

    private void TryAddToInventory(
        ItemDataBlobArray blobRef, 
        Entity character, 
        Entity collectable)
    {
        if (!CharacterContainersLookup.TryGetComponent(character, out WithCharacterContainers containers))
            return;

        Entity backpackContainer = containers.InventoryContainer;
        Entity weaponEquipmentContainer = containers.WeaponEquipmentContainer;
        Entity consumableEquipmentContainer = containers.ConsumableEquipmentContainer;

        var backpackBuffer = ContainerBufferLookup[backpackContainer];
        var weaponEquipmentBuffer = ContainerBufferLookup[weaponEquipmentContainer];
        var consumableEquipmentBuffer = ContainerBufferLookup[consumableEquipmentContainer];

        // Try get spawner
        SpawnedByLookup.TryGetComponent(collectable, out var spawner);

        var collectableId = CurrentItemIdLookup[collectable].Value;
        var collectableType = blobRef.Value.Value.ItemDataArray[collectableId].Type;

        switch (collectableType)
        {
            case ItemType.Weapon :
                AddToEqipment(
                    weaponEquipmentBuffer, 
                    collectable, 
                    weaponEquipmentContainer
                );
                UpdateContainerVersion(weaponEquipmentContainer);
                break;
            case ItemType.Spell :
                AddToBackpack(
                    backpackBuffer, 
                    character, 
                    collectable, 
                    collectableId, 
                    blobRef, 
                    backpackContainer
                );
                UpdateContainerVersion(backpackContainer);
                break;
            case ItemType.Consumable :
                AddToEqipment(
                    consumableEquipmentBuffer, 
                    collectable, 
                    consumableEquipmentContainer
                );
                UpdateContainerVersion(consumableEquipmentContainer);
                break;
            default :
                break;
        }
    }

    private void UpdateContainerVersion(Entity container)
    {
        var request = ECB.CreateEntity();
        
        ECB.AddComponent(request, new UpdateContainerVersion 
        { 
            Container = container 
        });
    }

    private void AddToEqipment(
            DynamicBuffer<ContainerBuffer> buffer,
            Entity collectableItem,
            Entity equipmentContainer
        )
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].ItemEntity != Entity.Null)
                continue;

            buffer[i] = new ContainerBuffer 
            { 
                ItemEntity = collectableItem,
                Quantity = 1 
            };

            MoveToCharacterContainer(collectableItem, equipmentContainer);
            TryChangeSpawnerState(collectableItem);

            return;
        }
    }

    private void AddToBackpack(
        DynamicBuffer<ContainerBuffer> backpackBuffer,
        Entity character,
        Entity collectable,
        ItemId collectableId,
        ItemDataBlobArray blobRef,
        Entity backpackContainer)
    {
        for (int i = 0; i < backpackBuffer.Length; i++)
        {
            var bufferItem = backpackBuffer[i];

            // CASE 1: slot not empty try stack
            if (bufferItem.ItemEntity != Entity.Null)
            {
                ItemId bufferItemId = CurrentItemIdLookup[bufferItem.ItemEntity].Value;
                int maxStack = blobRef.Value.Value.ItemDataArray[bufferItemId].MaxStack;

                if (bufferItemId == collectableId)
                {
                    if (bufferItem.Quantity < maxStack)
                    {
                        bufferItem.Quantity++;
                        backpackBuffer[i] = bufferItem;
                        MoveToCharacterContainer(collectable, backpackContainer);
                        TryChangeSpawnerState(collectable);
                        return;
                    }
                }
                
                continue;
            }
            
            // CASE 2: empty slot insert
            bufferItem.ItemEntity = collectable;
            bufferItem.Quantity = 1;
            backpackBuffer[i] = bufferItem;
            MoveToCharacterContainer(collectable, backpackContainer);
            TryChangeSpawnerState(collectable);
            return;
        }
    }

    private void TryChangeSpawnerState(
        Entity collectable)
    {
        if (SpawnedByLookup.HasComponent(collectable))
        {
            var spawnedBy = SpawnedByLookup[collectable].Entity;

            ECB.AddComponent(spawnedBy, new CurrentSpawnerState
            {
                Value = SpawnerState.Active 
            });
        }
    }

    private void MoveToCharacterContainer(
        Entity itemEntity, 
        Entity containerEntity)
    {      
        // Change current item state on inventory
        var changeItemStateReq = ECB.CreateEntity();
        
        ECB.AddComponent(changeItemStateReq, new ChangeCurrentItemState 
        { 
            ItemEntity = itemEntity,
            NewState = ItemState.Inventory
        });

        ECB.AddComponent(itemEntity, new ContainerEntityReference
        {
            Entity = containerEntity
        });

        ECB.RemoveComponent<WorldItemTag>(itemEntity);
        ECB.RemoveComponent<DroppedItemTag>(itemEntity);
    }
}
