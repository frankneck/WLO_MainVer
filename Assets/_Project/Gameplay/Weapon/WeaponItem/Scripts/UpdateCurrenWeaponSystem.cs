// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;

// public partial struct UpdateCurrentWeaponSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (request, currentStuff, entity) in SystemAPI
//             .Query<SelectedWeaponRequest, RefRW<CurrentStuff>>()
//             .WithEntityAccess())
//         {
//             // for character 
//             currentStuff.ValueRW.Item = request.ChoosedStuff;
            
//             // for equiped


//             ecb.RemoveComponent<SelectedWeaponRequest>(entity);
//         }
        
//         ecb.Playback(state.EntityManager);
//     }
// } 