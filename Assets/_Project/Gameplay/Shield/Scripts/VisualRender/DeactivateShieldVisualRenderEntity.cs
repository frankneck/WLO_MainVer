// using Unity.Collections;
// using Unity.Entities;

// // [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// [UpdateInGroup(typeof(PresentationSystemGroup))]
// public partial struct DeactivateShieldVisualRenderEntity : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (visualShield, shieldState, entity) in SystemAPI
//             .Query<VisualRenderShieldEntityReference, CharacterShieldState>()
//             .WithEntityAccess())
//         {            
//             if (!shieldState.IsActive && SystemAPI.Exists(visualShield.Entity))
//             {
//                 ecb.AddComponent<DestroyVisualRenderEntityTag>(visualShield.Entity);
                
//                 ecb.RemoveComponent<VisualRenderShieldActivated>(entity);
//                 ecb.RemoveComponent<VisualRenderShieldEntityReference>(entity);
//             }
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose(); 
//     }
// }