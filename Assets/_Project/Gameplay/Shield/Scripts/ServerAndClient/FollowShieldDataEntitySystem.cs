// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;

// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// public partial struct FollowShieldDataEntitySystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<FirstPersonCharacterComponent>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (transform, owner) in SystemAPI
//             .Query<RefRW<LocalTransform>, RefRO<GhostCharacterTarget>>()
//             .WithAll<PredictedShieldTag>())
//         {
//             if (!SystemAPI.HasComponent<FirstPersonCharacterComponent>(owner.ValueRO.Entity)) continue;

//             var character = SystemAPI.GetComponent<FirstPersonCharacterComponent>(owner.ValueRO.Entity);
//             var characterTransform = SystemAPI.GetComponent<LocalTransform>(owner.ValueRO.Entity);

//             float3 cameraOffset = new float3(0, 0, 0);
//             float3 cameraPosition = characterTransform.Position + cameraOffset;

//             float3 forward = math.mul(characterTransform.Rotation, math.forward(character.ViewLocalRotation));
// ;
//             float3 targetPosition = cameraPosition + forward * 1.5f;

//             quaternion targetRotation = quaternion.LookRotationSafe(forward, math.up());

//             transform.ValueRW.Position = math.lerp(transform.ValueRO.Position, targetPosition, 0.2f);
//             transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRotation, 0.2f);
//         }
//     }
// }