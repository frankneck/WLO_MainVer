// using Unity.Entities;
// using Unity.NetCode;
// using Unity.Collections;
// using Unity.Burst;

// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// public partial struct InitCollectableItemSpawnerSystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<NetworkTime>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         var networkTime = SystemAPI.GetSingleton<NetworkTime>();

//         var currentTick = networkTime.ServerTick;   

//         foreach (var (targetTick, entity) in SystemAPI
//             .Query<RefRW<SpawnerTargetTick>>()
//             .WithAll<SpawnerTag>()
//             .WithNone<SpawnerInitialized>()
//             .WithEntityAccess())
//         {
//             targetTick.ValueRW.Tick = currentTick;
//             ecb.AddComponent<SpawnerInitialized>(entity);
//         } 

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// } 