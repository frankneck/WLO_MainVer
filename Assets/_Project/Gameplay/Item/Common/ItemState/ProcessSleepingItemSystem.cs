// using Unity.Burst;
// using Unity.Entities;
// using Unity.Physics;
// using Unity.Physics.Systems;

// [UpdateInGroup(typeof(PhysicsSystemGroup))]
// [UpdateBefore(typeof(PhysicsSimulationGroup))]
// [BurstCompile]
// public partial struct ProcessSleepingItemSystem : ISystem
// {
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<PhysicsVelocity>();
//         state.RequireForUpdate<DroppedItemTag>();
//     }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         state.Dependency = new ProcessSleepingItemJob()
//             .ScheduleParallel(state.Dependency);
//     }
// }

// [WithAll(typeof(DroppedItemTag))]
// [BurstCompile]
// public partial struct ProcessSleepingItemJob : IJobEntity
// {
//     public void Execute(
//         ref PhysicsVelocity physicsVelocity
//     )
//     {
//         physicsVelocity.Angular = 0;
//     }
// }