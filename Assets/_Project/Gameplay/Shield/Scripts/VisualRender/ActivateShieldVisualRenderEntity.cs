// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;
// using Unity.Transforms;

// // [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// [UpdateInGroup(typeof(PresentationSystemGroup))]
// public partial struct ActivateShieldVisualRenderEntity : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<VisualPrefabs>();   
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var visualPrefabs = SystemAPI.GetSingleton<VisualPrefabs>();
        
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (transform, characterShieldState, entity) in SystemAPI
//             .Query<RefRO<LocalTransform>, CharacterShieldState>()
//             .WithAll<CharacterTag, GhostOwnerIsLocal>() // Only local client can view it
//             .WithNone<VisualRenderShieldActivated>()
//             .WithEntityAccess()) 
//         {
//             if (characterShieldState.IsActive)
//             {
//                 var createShieldVisual = ecb.Instantiate(visualPrefabs.Shield);
                
//                 ecb.SetComponent(createShieldVisual, transform.ValueRO);
//                 ecb.AddComponent(createShieldVisual, new VisualRenderCharacterTarget { Entity = entity });

//                 ecb.AddComponent(entity, new VisualRenderShieldEntityReference { Entity = createShieldVisual });
//                 ecb.AddComponent<VisualRenderShieldActivated>(entity);
//             }
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose(); 
//     }
// }