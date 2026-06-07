
// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;

// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct ProccessClientIsDeadSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (rpc, receive, entity) in SystemAPI
//             .Query<PlayerDeadNotificationToClient, ReceiveRpcCommandRequest>()
//             .WithEntityAccess())
//         {
//             ecb.AddComponent<PendingRespawnPlayerTag>(rpc.Player);
//             ecb.DestroyEntity(entity);
//         }

//         ecb.Playback(state.EntityManager);
//     } 
// } 
 
// /// <summary>
// /// Used to display player respawn screen
// /// </summary>
// public struct PendingRespawnPlayerTag : IComponentData { } 