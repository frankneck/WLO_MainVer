// using Unity.Collections;
// using Unity.Entities;

// /// <summary>
// /// The system receive request from UI input system 
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct ReceiveUIStateRequestSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (request, entity) in SystemAPI
//             .Query<UIStateRequest>()
//             .WithEntityAccess())
//         {
//             var previous = UIController.Instance.GetCurrentWindow();

//             var next = ResolveWindowState(request.Action, request.Window, previous);
            
//             ApplyWindowSideEffects(ref ecb, previous, next);

//             UIController.Instance.Apply(next);
            
//             ecb.RemoveComponent<UIStateRequest>(entity);
            
//             ecb.AddComponent<UIInputLockRequest>(entity);
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }

//     private WindowType ResolveWindowState(
//         UIAction action,
//         WindowType requested,
//         WindowType current)
//     {
//         switch (action)
//         {
//             case UIAction.Open:
//                 return requested;

//             case UIAction.Close:
//                 return WindowType.None;

//             case UIAction.Toggle:
//                 return ToggleWindow(requested, current);

//             default:
//                 return current;
//         }
//     }

//     private WindowType ToggleWindow(
//         WindowType requested,
//         WindowType current)
//     {
//         if (requested == WindowType.MenuWindow)
//         {
//             return current == WindowType.None ? WindowType.MenuWindow : WindowType.None;
//         }

//         return current == requested ? WindowType.None : requested;
//     }

//     private void ApplyWindowSideEffects(
//         ref EntityCommandBuffer ecb,
//         WindowType previous,
//         WindowType next)
//     {
//         if (previous == WindowType.InventoryWindow &&
//             next != WindowType.InventoryWindow)
//         {
//             CreateCloseInventoryRequest(ref ecb);
//         }

//         if (previous != WindowType.InventoryWindow &&
//             next == WindowType.InventoryWindow)
//         {
//             CreateOpenInventoryRequest(ref ecb);
//         }
//     }
        

//     private void CreateOpenInventoryRequest(
//         ref EntityCommandBuffer ecb)
//     {
//         var entity = ecb.CreateEntity();
//         ecb.AddComponent<OpenInventoryRequest>(entity);
//     }

//     private void CreateCloseInventoryRequest(
//         ref EntityCommandBuffer ecb)
//     {
//         var entity = ecb.CreateEntity();
//         ecb.AddComponent<CloseInventoryRequest>(entity);
//     }
// } 
