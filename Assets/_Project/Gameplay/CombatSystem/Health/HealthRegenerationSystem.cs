// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.NetCode;

// [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
// public partial struct HealthApplySystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var deltaTime = SystemAPI.Time.DeltaTime;
//         foreach (var (speed, hp, maxHp) in SystemAPI
//             .Query<RefRO<HealthRegenerationSpeed>, RefRW<CurrentHealth>, RefRO<MaxHealth>>()
//             .WithAll<CharacterTag>())
//         {
//             var t = deltaTime * speed.ValueRO.Value;
//             hp.ValueRW.Value = math.lerp(hp.ValueRW.Value, maxHp.ValueRO.Value, t);
//         }
//     }
// }

