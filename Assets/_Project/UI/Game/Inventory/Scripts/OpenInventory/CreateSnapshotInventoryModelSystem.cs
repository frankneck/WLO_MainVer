using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class CreateSnapshotInventoryModelSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp); 

        foreach (var (request, entity) in SystemAPI
            .Query<UpdateUIInventory>()
            .WithEntityAccess())
        {
            Entity inventoryContainer = request.InventoryContainer;
            Entity weaponEquipmentContainer = request.WeaponEquipmentContainer;
            Entity consumableEquipmentContainer = request.ConsumableEquipmentContainer;
            
            var inventoryBuffer = SystemAPI.GetBuffer<ContainerBuffer>(inventoryContainer);
            var weaponEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(weaponEquipmentContainer);
            var consumableEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(consumableEquipmentContainer);

            InventoryController.Instance.CreateInventorySnapshotModel(
                EntityManager, 
                
                inventoryContainer: inventoryContainer, 
                weaponEquipmentContainer: weaponEquipmentContainer,
                consumableEquipmentContainer: consumableEquipmentContainer,
                
                inventoryBuffer: inventoryBuffer,
                weaponEquipmentBuffer: weaponEquipmentBuffer,
                consumableContainerBuffer: consumableEquipmentBuffer
            );

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
    }
}