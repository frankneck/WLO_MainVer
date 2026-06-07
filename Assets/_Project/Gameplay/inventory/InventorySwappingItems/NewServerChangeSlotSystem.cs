    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.NetCode;
    using UnityEngine;

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct NewServerChangeSlotSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ItemDataBlobArray>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var blobRef = SystemAPI.GetSingleton<ItemDataBlobArray>();

            foreach (var (commands, receive, entity) in SystemAPI
                .Query<RpcInventoryCommands, ReceiveRpcCommandRequest>()
                .WithEntityAccess())
            {
                foreach (var rpc in commands.Commands)
                {
                    
    // #if UNITY_EDITOR
    //                 Debug.Log(
    //                     $"[SERVER] RPC RECEIVED | " +
    //                     $"SourceIndex={rpc.SourceIndex} " +
    //                     $"SourceType={rpc.SourceType} " +
    //                     $"SourceOwner={rpc.SourceOwner} " +
    //                     $"TargetIndex={rpc.TargetIndex} " +
    //                     $"TargetType={rpc.TargetType} " +
    //                     $"TargetOwner={rpc.TargetOwner}" +
    //                     $"Connection={receive.SourceConnection}" 
    //                 );
    // #endif

                    // Getting player
                    var connection = receive.SourceConnection;
                    var player = SystemAPI.GetComponent<PlayerEntityReference>(connection).Entity;
                    var playerCharacter = SystemAPI.GetComponent<FirstPersonPlayer>(player).ControlledCharacter;

                    // Getting player character's containers
                    WithCharacterContainers containers = SystemAPI.GetComponent<WithCharacterContainers>(playerCharacter);
                    
                    var inventoryBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.InventoryContainer);
                    var weaponEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.WeaponEquipmentContainer);
                    var consumableEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.WeaponEquipmentContainer);

                    if (!SystemAPI.HasBuffer<ContainerBuffer>(rpc.SourceOwner) || !SystemAPI.HasBuffer<ContainerBuffer>(rpc.TargetOwner))
                    {
                        // Debug.Log("Owner hasn' buffer");
                        ecb.DestroyEntity(entity);
                        continue;
                    }

                    // TODO: Validation of slot index (to avoid Index out of range)

                    var sourceBuffer = SystemAPI.GetBuffer<ContainerBuffer>(rpc.SourceOwner);
                    var targetBuffer = SystemAPI.GetBuffer<ContainerBuffer>(rpc.TargetOwner);

                    var sourceItem = sourceBuffer[rpc.SourceIndex].ItemEntity;
                    var targetItem = targetBuffer[rpc.TargetIndex].ItemEntity;

                    if (!CanPlaceItem(ref state, in blobRef, sourceItem, rpc.TargetType)
                        || !CanPlaceItem(ref state, in blobRef, targetItem, rpc.SourceType))
                    {
                        // Debug.Log("We can't place item");
                        ecb.DestroyEntity(entity);
                        continue;
                    }
                    
                    MoveItem(ref state, blobRef, rpc.SourceIndex, rpc.TargetIndex, ref sourceBuffer, ref targetBuffer);
                    
                    UpdateContainerVersion(ref ecb, rpc.SourceOwner);
                    UpdateContainerVersion(ref ecb, rpc.TargetOwner);
                    
                    ecb.DestroyEntity(entity);   
                }
            }
            
            ecb.Playback(state.EntityManager);
        }

    #region Additional methods
    
    
    private void UpdateContainerVersion(
        ref EntityCommandBuffer ecb, 
        Entity container)
    {
        var request = ecb.CreateEntity();
        
        ecb.AddComponent(request, new UpdateContainerVersion 
        { 
            Container = container 
        });
    }

    /// <summary>
    /// Converts slot type to slotmask
    /// </summary>
    private AllowedSlots SlotTypeToMask(SlotType slotType) => slotType switch
    {
        SlotType.WeaponEquipmentSlot => AllowedSlots.WeaponEquipmentSlots,
        SlotType.ConsumableEquipmentSlot => AllowedSlots.ConsumableEquipmentSlots,
        SlotType.InventorySlot => AllowedSlots.InventorySlots,
        SlotType.WeaponSlot => AllowedSlots.WeaponSlots,
        _ => AllowedSlots.None
    };

    private bool CanPlaceItem(
        ref SystemState state,
        in ItemDataBlobArray blobRef,
        Entity item,
        SlotType targetSlotType)
    {
        if (item == Entity.Null)
            return true;
        
        if (!SystemAPI.HasComponent<CurrentItemId>(item))
            return false;

        var itemId = SystemAPI.GetComponent<CurrentItemId>(item).Value;
        var itemData = blobRef.Value.Value.ItemDataArray[itemId];
        var targetMask = SlotTypeToMask(targetSlotType);

        return (itemData.AllowedSlots & targetMask) != 0;
    }


    /// <summary>
    /// Main logic method swap, combine or do something else an item
    /// </summary>
    private void MoveItem(
        ref SystemState state,
        ItemDataBlobArray blobRef,
        int sourceIndex,
        int targetIndex,
        ref DynamicBuffer<ContainerBuffer> sourceBuffer,
        ref DynamicBuffer<ContainerBuffer> targetBuffer)
    {
        if (sourceBuffer[sourceIndex].ItemEntity == Entity.Null)
            return;

        var sourceItem = sourceBuffer[sourceIndex];
        var targetItem = targetBuffer[targetIndex];

        Entity sourceItemEntity = sourceItem.ItemEntity;
        Entity targetItemEntity = targetItem.ItemEntity;
        
        // Combine but target is null
        if (targetBuffer[targetIndex].ItemEntity == Entity.Null)
        {
            targetItem.ItemEntity = sourceItemEntity;
            targetItem.Quantity = sourceItem.Quantity;
            targetBuffer[targetIndex]= targetItem;

            // Cleaning source 
            sourceBuffer[sourceIndex] = default;
            // Debug.Log("SWAP when target is null");
            return;
        }

        ItemId sourceItemId = SystemAPI.GetComponent<CurrentItemId>(sourceItemEntity).Value;
        ItemId targetItemId = SystemAPI.GetComponent<CurrentItemId>(targetItemEntity).Value;

        // Swap
        if (targetItemId != sourceItemId)
        {
            targetBuffer[targetIndex]= sourceItem;
            sourceBuffer[sourceIndex]= targetItem;
            // Debug.Log("SWAP");
            return;
        }

        var sourceMaxStack = blobRef.Value.Value.ItemDataArray[sourceItemId].MaxStack;
        var targetMaxStack = blobRef.Value.Value.ItemDataArray[targetItemId].MaxStack;
        var sum = sourceItem.Quantity + targetItem.Quantity;
        
        // Swap if one of slot is full 
        if (targetItemId == sourceItemId && 
            (targetItem.Quantity == targetMaxStack 
            || sourceItem.Quantity == sourceMaxStack))
        {
            targetBuffer[targetIndex]= sourceItem;
            sourceBuffer[sourceIndex]= targetItem;    
            // Debug.Log("SWAP if full");
            return;
        }

        // Combine 
        if (sum <= targetMaxStack)
        {
            targetItem.Quantity = sum;
            targetBuffer[targetIndex] = targetItem;
            // Debug.Log("COMBINE");
            
            // Cleaning source 
            sourceBuffer[sourceIndex] = default;
        }
        else
        {
            targetItem.Quantity = targetMaxStack;
            targetBuffer[targetIndex] = targetItem;
            // Debug.Log("JUST COMBINE AND REMAINGN");

            // Filling remaining in source
            sourceItem.Quantity = sum - targetMaxStack;
            sourceBuffer[sourceIndex] = sourceItem;
        }
    }


#endregion
}