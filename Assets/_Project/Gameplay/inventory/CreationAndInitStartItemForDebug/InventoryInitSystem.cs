
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;
// using UnityEngine.Rendering;

// /// <summary>
// /// Moves created items to player character backpack
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]
// public partial struct MoveCreatedItemsToInventorySystem : ISystem
// {
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<ReadyToAssignInBuffer>();    
//         state.RequireForUpdate<ItemDataBlobArray>();
//     }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = SystemAPI
//             .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
//             .CreateCommandBuffer(state.WorldUnmanaged);

//         var blobRef = SystemAPI.GetSingleton<ItemDataBlobArray>();

//         var moveJob = new MoveCreatedItemsToInventoryJob
//         {
//             ContainersLookup = SystemAPI.GetComponentLookup<WithCharacterContainers>(true),
//             CurrentItemIdLookup = SystemAPI.GetComponentLookup<CurrentItemId>(true),
//             blobRef = blobRef,
//             ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
//             ECB = ecb
//         };

//         state.Dependency = moveJob.Schedule(state.Dependency);
//     }
// }

// [BurstCompile]
// public partial struct MoveCreatedItemsToInventoryJob : IJobEntity
// {
//     [ReadOnly] public ComponentLookup<WithCharacterContainers> ContainersLookup;
//     [ReadOnly] public ComponentLookup<CurrentItemId> CurrentItemIdLookup; 
//     [ReadOnly] public ItemDataBlobArray blobRef;
//     public BufferLookup<ContainerBuffer> ContainerBufferLookup;
//     public EntityCommandBuffer ECB;

//     public void Execute( 
//        in CharacterItemOwner targetEntity,
//         ReadyToAssignInBuffer request,
//         Entity entity)
//     {
//         if (!ContainersLookup.TryGetComponent(targetEntity.Entity, out var containers)) 
//             return;

//         Entity backpackContainer = containers.InventoryContainer;
//         Entity equipmentContainer = containers.WeaponEquipmentContainer;

//         if (!ContainerBufferLookup.TryGetBuffer(backpackContainer, out var inventoryBuffer))
//             return;

//         if (!ContainerBufferLookup.TryGetBuffer(equipmentContainer, out var equipmentBuffer))
//             return;

//         var itemId = CurrentItemIdLookup[entity].Value;
//         var itemType = blobRef.Value.Value.ItemDataArray[itemId].Type;

//         switch (itemType)
//         {
//             case ItemType.Weapon :
//                 AddItemEntityToBuffer(ref equipmentBuffer, ref entity);
//                 break;
//             case ItemType.Spell : 
//                 AddItemEntityToBuffer(ref inventoryBuffer, ref entity);
//                 break;
//             default :
//                 break;
//         }
        
//         ECB.RemoveComponent<ReadyToAssignInBuffer>(entity);
//     }

//     private void AddItemEntityToBuffer(
//             ref DynamicBuffer<ContainerBuffer> buffer,
//             ref Entity entity
//         )
//     {
//         for (int i = 0; i < buffer.Length; i++)
//         {
//             if (buffer[i].Item != Entity.Null)
//                 continue;

//             buffer[i] = new ContainerBuffer 
//             { 
//                 Item = entity,
//                 Count = 1 
//             };

//             break;
//         }
//     }
// }