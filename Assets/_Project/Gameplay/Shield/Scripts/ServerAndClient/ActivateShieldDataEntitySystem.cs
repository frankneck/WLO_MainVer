// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;
// using Unity.Transforms;

// /// <summary>
// /// Description: instantiates shield data entity and set position to it, set shiled's ghost character owner, add IsSpawnedTag
// /// 
// /// Flow: ActivateShieldDataEntitySystem -> ShieldFollowSystem
// /// 
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// public partial struct ActivateShieldDataEntitySystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<ShieldPrefab>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         var shieldPrefab = SystemAPI.GetSingleton<ShieldPrefab>().Shield;

//         foreach (var (transform, shieldState, entity) in SystemAPI
//             .Query<RefRO<LocalTransform>, CharacterShieldState>()
//             .WithNone<ShieldActivated>()
//             .WithEntityAccess())
//         {
//             if (shieldState.IsActive)
//             {
//                 var shield = ecb.Instantiate(shieldPrefab);
                
//                 ecb.SetComponent(shield, new ShieldState { IsActive = shieldState.IsActive });
//                 ecb.AddComponent(shield, new GhostCharacterTarget { Entity = entity });
//                 ecb.SetComponent(shield, transform.ValueRO);
                
//                 ecb.AddComponent<ShieldActivated>(entity);
//             }
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }

