
// using Unity.Collections;
// using Unity.Entities;

// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// public partial struct PreDeactivateShieldDataEntitySystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<CharacterShieldState>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (shieldState, ghostCharacterTarget) in SystemAPI
//             .Query<RefRW<ShieldState>, GhostCharacterTarget>())
//         {
//             if (!SystemAPI.HasComponent<CharacterShieldState>(ghostCharacterTarget.Entity)) continue;

//             var inputStateValue = SystemAPI.GetComponent<CharacterShieldState>(ghostCharacterTarget.Entity);

//             if (!inputStateValue.IsActive)
//             {
//                 shieldState.ValueRW.IsActive = inputStateValue.IsActive; 
//             }
//         }
//     }
// }


// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// public partial struct DeactivateShieldDataEntitySystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (shieldState, GhostCharacterTarget, entity) in SystemAPI
//             .Query<ShieldState, GhostCharacterTarget>()
//             .WithEntityAccess())
//         {
//             if (!shieldState.IsActive)
//             {
//                 if (SystemAPI.Exists(GhostCharacterTarget.Entity))
//                 {
//                     ecb.AddComponent<DestroyEntityTag>(entity);
//                     ecb.RemoveComponent<GhostCharacterTarget>(entity);
//                 }

//                 ecb.RemoveComponent<ShieldActivated>(GhostCharacterTarget.Entity);
//             }
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }