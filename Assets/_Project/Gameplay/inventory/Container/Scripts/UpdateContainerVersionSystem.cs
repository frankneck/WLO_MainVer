using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct UpdateContainerVersionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI
            .Query<UpdateContainerVersion>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(request.Container))
                continue;

            if (!SystemAPI.HasComponent<ContainerVersion>(request.Container))
                continue;

            var containerVersion = SystemAPI.GetComponentRW<ContainerVersion>(request.Container);
            containerVersion.ValueRW.Value++;

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct UpdateContainerVersion : IComponentData
{
    public Entity Container;
} 