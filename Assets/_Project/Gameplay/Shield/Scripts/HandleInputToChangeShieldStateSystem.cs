// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;

// /// <summary>
// /// Description: handle input to change game data shield change.
// /// 
// /// Group: PredictedSimulationSystemGruop because predicted ghost prefab
// /// 
// /// Flow: HandleInputToChangeShieldStateSystem -> ShieldSpawnSystem -> ShieldFollowSystem
// /// </summary>

// [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
// public partial struct HandleInputToChangeShieldStateSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (input, shieldState, entity) in SystemAPI
//             .Query<RefRO<WeaponControl>, RefRW<CharacterShieldState>>()
//             .WithEntityAccess())
//         {
//             shieldState.ValueRW.IsActive = input.ValueRO.ShieldHeld;
//         }
//     }
// }