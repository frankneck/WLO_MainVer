using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
// [BurstCompile]
public partial struct ChangeCurrentItemStateSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI
            .Query<ChangeCurrentItemState>()
            .WithEntityAccess())
        {
            if (SystemAPI.HasComponent<CurrentItemState>(request.ItemEntity))
            {
                ecb.SetComponent(request.ItemEntity, new CurrentItemState
                {
                    Value = request.NewState 
                });
            }
            
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}