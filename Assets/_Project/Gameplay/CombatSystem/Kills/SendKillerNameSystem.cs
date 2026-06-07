// using Unity.Collections;
// using Unity.Entities;

// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [UpdateAfter(typeof(ApplyDamageSystem))]
// public partial struct SendKillerNameSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (damageEvent, entity) in SystemAPI
//             .Query<DamageEvent>()
//             .WithEntityAccess())
//         {
//             // Getting character
//             var dealingEntity = damageEvent.Dealed;
//             var receivingEntity = damageEvent.Received;

//             // Getting Network
//             if (!SystemAPI.HasComponent<NetworkEntityReference>(dealingEntity) || 
//                 !SystemAPI.HasComponent<NetworkEntityReference>(receivingEntity)) continue;

            

//             UnityEngine.Debug.Log($"[SendKillerNameSystem] Killer {killerName} of {victim}");
            
//             ecb.AddComponent(victim.Entity, new KillerName { Value = killerName.Value });
            
//             // TODO: Store Killer and Victim to do something Interesting :)
//             ecb.RemoveComponent<Victim>(entity);
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }