// using Unity.Burst;
// using Unity.Entities;

// /// <summary>
// /// After character has been fully initialized player need to assign for define character and update his current state.
// /// Updates current state.
// /// </summary>
// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]
// public partial struct SecondPlayerInitializationSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
//             .CreateCommandBuffer(state.WorldUnmanaged);

//         var jobHandle = new SecondPlayerInitializationJob
//         {
//             ECB = ecb
//         };

//         state.Dependency = jobHandle.Schedule(state.Dependency);
//     }
// }

// [BurstCompile]
// public partial struct SecondPlayerInitializationJob : IJobEntity
// {
//     public EntityCommandBuffer ECB;

//     public void Execute(
//         in AssignCharacterToPlayer request,
//         Entity requestEntity
//     )
//     {
//         // var characterEntity = request.CharacterEntity;
//         var playerEntity = request.PlayerEntity;
        
//         PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
//             ref ECB, 
//             playerEntity, 
//             PlayerState.Playing);

//         ECB.DestroyEntity(requestEntity);
//     }
// }