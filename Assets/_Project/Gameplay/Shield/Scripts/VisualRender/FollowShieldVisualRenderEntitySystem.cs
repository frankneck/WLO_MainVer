// using Unity.NetCode;
// using Unity.Entities;
// using Unity.Transforms;
// using Unity.Mathematics;

// [UpdateInGroup(typeof(PresentationSystemGroup))]
// public partial struct FollowShieldVisualRenderEntitySystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<FirstPersonCharacterComponent>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (transform, character) in SystemAPI
//             .Query<RefRW<LocalTransform>, RefRO<VisualRenderCharacterTarget>>())
//         {
//             if (!SystemAPI.HasComponent<FirstPersonCharacterComponent>(character.ValueRO.Entity)) continue;

//             var characterComponent = SystemAPI.GetComponent<FirstPersonCharacterComponent>(character.ValueRO.Entity);
//             var characterTransform = SystemAPI.GetComponent<LocalTransform>(character.ValueRO.Entity);

//             float3 cameraOffset = new float3(0, 0, 0);
//             float3 cameraPosition = characterTransform.Position + cameraOffset;

//             float3 forward = math.mul(characterTransform.Rotation, math.forward(characterComponent.ViewLocalRotation));
//             float3 position = cameraPosition + forward * 1.5f;

//             quaternion rotation = quaternion.LookRotationSafe(forward, math.up());

//             transform.ValueRW.Position = position;
//             transform.ValueRW.Rotation = rotation;
//         }
//     }
// }