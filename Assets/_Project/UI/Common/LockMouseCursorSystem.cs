// using Unity.Collections;
// using Unity.Entities;
// using UnityEngine;

// /// <summary>
// /// The system is responsible for mouse locking independs of current UI State 
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// [UpdateInGroup(typeof(PresentationSystemGroup))]
// public partial struct LockMouseCursorSystem : ISystem
// {
//     private EntityQuery _characterQuery;

//     public void OnCreate(ref SystemState state)
//     {
//         _characterQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalCharacterTag>().Build(state.EntityManager);
//         state.RequireForUpdate(_characterQuery);
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);
        
//         var currentState = UIController.Instance.GetCurrentWindow();    
                
//         foreach (var (_, entity) in SystemAPI
//             .Query<UIInputLockRequest>()
//             .WithEntityAccess())
//         {
//             switch (currentState)
//             {
//                 case WindowType.None :
//                     Cursor.lockState = CursorLockMode.Locked;
//                     Cursor.visible = false;
//                     break;
                
//                 case WindowType.MenuWindow : 
//                     Cursor.lockState = CursorLockMode.None;
//                     Cursor.visible = true;
//                     break;
                
//                 case WindowType.InventoryWindow :
//                     Cursor.lockState = CursorLockMode.None;
//                     Cursor.visible = true;
//                     break;
                
//                 case WindowType.Console :
//                     Cursor.lockState = CursorLockMode.None;
//                     Cursor.visible = true;
//                     break;
//             }
            
//             ecb.DestroyEntity(entity);
//         }
//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }