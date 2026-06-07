using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct InitContainerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach (var (selfRequest, entity) in SystemAPI
            .Query<InitContainerRequest>()
            .WithEntityAccess())
        {
            var container = selfRequest.ItemContainer;

            if (!SystemAPI.HasBuffer<ContainerBuffer>(container) ||
                !SystemAPI.HasComponent<ContainerTypeComponent>(container)) continue;

            // Change type of container
            var contaienrType = SystemAPI.GetComponentRW<ContainerTypeComponent>(container);
            contaienrType.ValueRW.Value = selfRequest.Type;

            // Initialization of container
            for (int i = 0; i < selfRequest.Size; i++)
            {
                ecb.AppendToBuffer(container, new ContainerBuffer { 
                    ItemEntity = Entity.Null,
                    Quantity = 0
                });
            }

            if (contaienrType.ValueRW.Value == ContainerType.Weapon)
            {
                var needFillContainerReq = ecb.CreateEntity();
                ecb.AddComponent(needFillContainerReq, new FillWeaponContainer
                {
                    Weapon = selfRequest.Item,
                    Container = selfRequest.ItemContainer 
                });  
            }

            ecb.RemoveComponent<InitContainerRequest>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct FillWeaponContainer : IComponentData
{
    public Entity Weapon;
    public Entity Container;

}