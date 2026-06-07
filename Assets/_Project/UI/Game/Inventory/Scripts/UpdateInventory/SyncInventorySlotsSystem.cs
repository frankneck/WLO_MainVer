using System.Collections.Generic;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class SyncInventorySlotsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<InventoryView>();
    }

    protected override void OnUpdate()
    {
        var inventoryView = SystemAPI.ManagedAPI.GetSingleton<InventoryView>();

        foreach (var (characterContainers, entity) in SystemAPI
            .Query<WithCharacterContainers>() 
            .WithEntityAccess())
        {
            var containers = characterContainers;

            if (!SystemAPI.HasBuffer<ContainerBuffer>(containers.WeaponEquipmentContainer) || 
                !SystemAPI.HasBuffer<ContainerBuffer>(containers.InventoryContainer) ||
                !SystemAPI.HasBuffer<ContainerBuffer>(containers.ConsumableEquipmentContainer))
                continue;

            var weaponEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.WeaponEquipmentContainer);
            var consumableEquipmentBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.ConsumableEquipmentContainer);
            var inventoryBuffer = SystemAPI.GetBuffer<ContainerBuffer>(containers.InventoryContainer);

            SyncSlotsAndBuffer(inventoryView, inventoryView.InventorySlots, inventoryBuffer);
            SyncSlotsAndBuffer(inventoryView, inventoryView.WeaponEquipmentSlots, weaponEquipmentBuffer);
            SyncSlotsAndBuffer(inventoryView, inventoryView.ConsumableEquipmentSlots, consumableEquipmentBuffer);
        }
    }

    private void SyncSlotsAndBuffer(
        InventoryView view, 
        IReadOnlyList<Slot> slots, 
        DynamicBuffer<ContainerBuffer> buffer)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < buffer.Length)
            {
                view.ShowSlot(SlotType.InventorySlot, i);
            }
            else
            {
                view.HideSlot(SlotType.InventorySlot, i);
            }
        }
    }
}