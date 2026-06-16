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
            var containerEntity = selfRequest.ItemContainer;

            if (!SystemAPI.HasBuffer<ContainerBuffer>(containerEntity) ||
                !SystemAPI.HasComponent<ContainerTypeComponent>(containerEntity)) continue;

            // Change type of container
            var contaienrType = SystemAPI.GetComponentRW<ContainerTypeComponent>(containerEntity);
            contaienrType.ValueRW.Value = selfRequest.Type;

            // Initialization of container
            for (int i = 0; i < selfRequest.Size; i++)
            {
                ecb.AppendToBuffer(containerEntity, new ContainerBuffer { 
                    ItemEntity = Entity.Null,
                    Quantity = 0
                });
            }
            
            switch (contaienrType.ValueRW.Value)
            {
                case ContainerType.CharacterWeaponEquipment :
                    SendSpawnPlayerFirstWeaponsRequest(ref ecb, containerEntity);
                    break;
                case ContainerType.Weapon :
                    SendFillWeaponContainerRequest(ref ecb, selfRequest.Item, selfRequest.ItemContainer);
                    break;
            }

            ecb.RemoveComponent<InitContainerRequest>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void SendSpawnPlayerFirstWeaponsRequest(
        ref EntityCommandBuffer ecb,
        Entity containerEntity
    )
    {
        var needToCreateeFirstWeaponRequest = ecb.CreateEntity();
        ecb.AddComponent(needToCreateeFirstWeaponRequest, new SpawnPlayerFirstWeaponsToPutIntoContainer
        {
            ContainerEntity = containerEntity 
        });
    }

    public void SendFillWeaponContainerRequest(
        ref EntityCommandBuffer ecb,
        Entity weaponEntity,
        Entity containerEntity
    )
    {
        // create request to create first spells for weapon container
        var needFillContainerReq = ecb.CreateEntity();
        ecb.AddComponent(needFillContainerReq, new FillWeaponContainer
        {
            Weapon = weaponEntity,
            Container = containerEntity 
        });  
    }
}

public struct FillWeaponContainer : IComponentData
{
    public Entity Weapon;
    public Entity Container;

}

public struct SpawnPlayerFirstWeaponsToPutIntoContainer : IComponentData
{
    public Entity ContainerEntity;
}