// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.NetCode;

// /// <summary>
// /// Sends rpc to the server 
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct ClientFastSwapInventorySystem : ISystem
// {
//     private EntityQuery _query;

//     public void OnCreate(ref SystemState state)
//     {
//         _query = new EntityQueryBuilder(Allocator.Temp).WithAll<NetworkStreamConnection>().Build(state.EntityManager); 
//         state.RequireForUpdate(_query);
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var connection = _query.GetSingletonEntity();
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (fastRequestComponent, requestEntity) in SystemAPI.Query<ClientFastRequest>().WithEntityAccess())
//         {
//             var rpc = ecb.CreateEntity();
//             ecb.AddComponent(rpc, new SendRpcCommandRequest { TargetConnection = connection });
//             ecb.AddComponent(rpc, new FromClientFastSwapInventoryRequest
//             {
//                 SourceIndex = fastRequestComponent.SourceIndex,
//                 SourceOwner = fastRequestComponent.SourceOwner,
//                 SourceType = fastRequestComponent.SourceType
//             });
        
//             ecb.RemoveComponent<ClientFastRequest>(requestEntity);
//         }
        
//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }

// /// <summary>
// /// Calculates target index and then sends rpc to the client   
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]
// public partial struct ServerFastSwapInventorySystem : ISystem
// {
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<ItemDataBlobArray>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);
//         var blobReference = SystemAPI.GetSingleton<ItemDataBlobArray>();

//         foreach (var (receive, rpc, entity) in SystemAPI
//             .Query<ReceiveRpcCommandRequest, FromClientFastSwapInventoryRequest>()
//             .WithEntityAccess())
//         {
//             // NetworkEntity has component Player Entity Reference that needed to get Player and its current stuff
//             if (!SystemAPI.HasComponent<PlayerEntityReference>(receive.SourceConnection)) 
//                 continue;
            
//             // Getting player's character
//             var player = SystemAPI.GetComponent<PlayerEntityReference>(receive.SourceConnection).Entity;
//             var character = SystemAPI.GetComponent<FirstPersonPlayer>(player).ControlledCharacter;
            
//             // Getting player character's containers
//             WithCharacterContainers containers = SystemAPI.GetComponent<WithCharacterContainers>(character);
           
//             Entity backpackContainer = containers.InventoryContainer;
//             Entity equipmentContainer = containers.EquipmentContainer;
           
//             var inventoryBuffer = SystemAPI.GetBuffer<ContainerBuffer>(backpackContainer);
//             var equipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(equipmentContainer);
            
//             // If Current Weapon is selected
//             if (SystemAPI.GetComponent<CurrentStuff>(character).Entity != Entity.Null)
//             {
//                 Entity weapon = SystemAPI.GetComponent<CurrentStuff>(character).Entity;
//                 Entity weaponContainer = SystemAPI.GetComponent<WithWeaponContainer>(weapon).Container;
//                 if (!SystemAPI.HasBuffer<ContainerBuffer>(weaponContainer))
//                 {
//                     UnityEngine.Debug.LogWarning("[ServerFastSwapInventorySystem] The system hasn't have spell buffer");
//                     ecb.DestroyEntity(entity);
//                     continue;   
//                 };
                
//                 var weaponBuffer = SystemAPI.GetBuffer<ContainerBuffer>(weaponContainer);

//                 switch (rpc.SourceType)
//                 {
//                     case SlotType.Inventory :
//                         MoveItem(ref state, blobReference, rpc.SourceIndex, ref inventoryBuffer, ref weaponBuffer);
//                         break;
//                     case SlotType.Weapon :
//                         var sourceContainer = SystemAPI.GetComponent<WithWeaponContainer>(rpc.SourceOwner).Container;
//                         var sourceBuffer = SystemAPI.GetBuffer<ContainerBuffer>(sourceContainer);
//                         MoveItem(ref state, blobReference, rpc.SourceIndex, ref sourceBuffer, ref inventoryBuffer);
//                         break;
//                     case SlotType.Equipment :
//                         MoveItem(ref state, blobReference, rpc.SourceIndex, ref equipmentBuffer, ref inventoryBuffer);
//                         break;
//                     default :
//                         break;
//                 }
//             }
//             else
//             {
//                 switch (rpc.SourceType)
//                 {
//                     case SlotType.Equipment :
//                         MoveItem(ref state, blobReference, rpc.SourceIndex, ref equipmentBuffer, ref inventoryBuffer);
//                         break;
//                     default :
//                         break;
//                 }
//             }


//             ecb.DestroyEntity(entity);
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }

//     private void MoveItem(
//         ref SystemState state,
//         ItemDataBlobArray blob,
//         int sourceIndex,
//         ref DynamicBuffer<ContainerBuffer> sourceBuffer,
//         ref DynamicBuffer<ContainerBuffer> targetBuffer)
//     {
//         Entity sourceItem = sourceBuffer[sourceIndex].Item;
//         ItemId sourceItemId = SystemAPI.GetComponent<CurrentItemId>(sourceItem).Value;

//         int remaining = sourceBuffer[sourceIndex].Count;

//         for (int i = 0; i < targetBuffer.Length; i++)
//         {
//             if (remaining == 0)
//                 break;

//             var targetItem = targetBuffer[i];

//             if (targetItem.Item == Entity.Null)
//                 continue;

//             ItemId targetItemId = SystemAPI.GetComponent<CurrentItemId>(targetItem.Item).Value;

//             if (targetItemId != sourceItemId)
//                 continue;

//             int maxStack = blob.Value.Value.ItemDataArray[targetItemId].MaxStack;

//             if (targetItem.Count >= maxStack)
//                 continue;

//             int canTake = maxStack - targetItem.Count;
//             int toMove = math.min(remaining, canTake);

//             targetItem.Count += toMove;
//             targetBuffer[i] = targetItem;
//             remaining -= toMove;
//         }

//         for (int i = 0; i < targetBuffer.Length; i++)
//         {
//             if (remaining == 0)
//                 break;

//             var targetItem = targetBuffer[i];

//             if (targetItem.Item != Entity.Null)
//                 continue;

//             int maxStack = blob.Value.Value.ItemDataArray[sourceItemId].MaxStack;

//             int toMove = math.min(maxStack, remaining);

//             targetItem.Item = sourceItem;
//             targetItem.Count = toMove;
//             targetBuffer[i] = targetItem;
//             remaining -= toMove;
//         }

//         if (remaining == 0)
//         {
//             sourceBuffer[sourceIndex] = default;
//         }
//         else
//         {
//             sourceBuffer[sourceIndex] = new ContainerBuffer
//             {
//                 Item = sourceItem,
//                 Count = remaining
//             };
//         }
//     }
// }