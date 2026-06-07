// using Unity.Entities;
// using Unity.Collections;

// // Update UI
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct DisplayRespawnWindowSystem : ISystem
// {
//     private int lastSeconds;

//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<HudView>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);
        
//         foreach (var (player, seconds, entity) in SystemAPI
//             .Query<RefRO<FirstPersonPlayer>, RefRO<LeftSecondsToRespawn>>()
//             .WithAll<PendingRespawnPlayerTag>()
//             .WithEntityAccess())
//         {
//             if (lastSeconds == seconds.ValueRO.Value)
//                 continue;

//             var hudView = SystemAPI.ManagedAPI.GetSingleton<HudView>();
            
//             if (seconds.ValueRO.Value > 0)
//             {
//                 hudView.DisplayRespawnWindow();
//                 hudView.RefreshTimer(seconds.ValueRO.Value);
//             }
//             else
//             {
//                 hudView.HideRespawnWindow();
//                 ecb.RemoveComponent<PendingRespawnPlayerTag>(entity);
//             }

//             lastSeconds = seconds.ValueRO.Value;
//         }
        
//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }