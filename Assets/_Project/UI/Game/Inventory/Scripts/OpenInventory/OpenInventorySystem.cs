using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct OpenInventorySystem : ISystem
{
    private EntityQuery _localCharacter;

    public void OnCreate(ref SystemState state)
    {
        _localCharacter = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalCharacterTag>()
            .Build(state.EntityManager);

        state.RequireForUpdate(_localCharacter);
        state.RequireForUpdate<WithCharacterContainers>();
    } 

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var localCharacter = _localCharacter.GetSingletonEntity();

        foreach (var (_, entity) in SystemAPI
            .Query<OpenInventoryRequest>()
            .WithEntityAccess())
        {
            bool isHasCharacterContainers = SystemAPI.HasComponent<WithCharacterContainers>(localCharacter);

            if (isHasCharacterContainers)
            {
                var containers = SystemAPI.GetComponent<WithCharacterContainers>(localCharacter);

                var inventoryContainer = containers.InventoryContainer;
                var weaponEquipmentContainer = containers.WeaponEquipmentContainer;
                var consumableEquipmentContainer = containers.ConsumableEquipmentContainer;

                if (!SystemAPI.Exists(inventoryContainer) || 
                    !SystemAPI.Exists(weaponEquipmentContainer) || 
                    !SystemAPI.Exists(consumableEquipmentContainer))
                {
                    // Containers haven't created yet
                    continue;
                }

                var newRequst = ecb.CreateEntity();
                ecb.AddComponent(newRequst, new UpdateUIInventory
                {
                    InventoryContainer = inventoryContainer,
                    WeaponEquipmentContainer = weaponEquipmentContainer,
                    ConsumableEquipmentContainer = consumableEquipmentContainer
                });
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}