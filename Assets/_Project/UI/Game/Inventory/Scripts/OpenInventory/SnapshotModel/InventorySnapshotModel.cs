using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InventorySnapshotModel
{
    public Container WeaponEquipmentContainer;
    public Container ConsumableEquipmentContainer;
    public Container InventoryContainer;
    private Dictionary<ContainerKey, Container> _containers;

#region  Public methods

    public InventorySnapshotModel(
        ref EntityManager em,
        ItemManagedDatabase itemManagedDatabase,
        Entity inventoryContainer,
        Entity weaponEquipmentContainer,
        Entity consumableEquipmentContainer,
        DynamicBuffer<ContainerBuffer> inventoryBuffer,
        DynamicBuffer<ContainerBuffer> weaponEquipmentBuffer,
        DynamicBuffer<ContainerBuffer> consumableEquipmentBuffer)
    {
        _containers = new Dictionary<ContainerKey, Container>();
        Debug.Log("[InventorySnapshotModel] InventorySnapshotModel has been created.");

        // Build equipments and inventory containers
        WeaponEquipmentContainer = BuildInventoryModelContainerWithContainers(em, "WeaponEquipment", itemManagedDatabase, weaponEquipmentContainer, weaponEquipmentBuffer);
        ConsumableEquipmentContainer = BuildInventoryModelContainer(em, "ConsumableEquipment", itemManagedDatabase, consumableEquipmentContainer, consumableEquipmentBuffer);
        InventoryContainer = BuildInventoryModelContainer(em, "Inventory", itemManagedDatabase, inventoryContainer, inventoryBuffer);

        SaveContainer(WeaponEquipmentContainer, SlotType.WeaponEquipmentSlot);
        SaveContainer(ConsumableEquipmentContainer, SlotType.ConsumableEquipmentSlot);
        SaveContainer(InventoryContainer, SlotType.InventorySlot);
    }

    public bool TryMove(Slot originalSlot, Slot closestSlot)
    {
        var fromContainer = ResolveContainer(originalSlot);
        var toContainer = ResolveContainer(closestSlot);

        // Case 1: we can't move item
        if (originalSlot.m_EntityOwner == closestSlot.m_EntityOwner &&
            originalSlot.m_Type == closestSlot.m_Type &&
            originalSlot.m_Index == closestSlot.m_Index) return false;

        Item sourceItem = fromContainer.Get(originalSlot.m_Index);
        Item targetItem = toContainer.Get(closestSlot.m_Index);

        if (CanStack(sourceItem, targetItem))
        {
            // Case 2: We can stack items
            Combine(fromContainer, originalSlot.m_Index, toContainer, closestSlot.m_Index);
            return true;
        }

        if (toContainer != null)
        {   
            // Case 3: we can't swap (different allowed slots)
            if (!toContainer.CanPlace(sourceItem, closestSlot))
            {
                return false;
            }

            // Case 4: we can't swap (different allowed slots and closest slot isn't empty)
            if (targetItem != null && !fromContainer.CanPlace(targetItem, originalSlot))
            {
                return false;
            }
            
            // Case 5: We can swap
            Swap(fromContainer, originalSlot.m_Index, toContainer, closestSlot.m_Index);
            return true;
        }

        Debug.Log($"[NewInventoryController] TryMove failed.");
        return false;
    }

    // Getting container by key
    public Container ResolveContainer(Slot slot)
    {
        var key = new ContainerKey
        {
            Owner = slot.m_EntityOwner,
            Type = slot.m_Type,
        };

        if (!_containers.TryGetValue(key, out var container))
            return null;
        
        return container;
    }

    public int Combine(IContainer originalContainer, int fromIndex, IContainer closestContainer, int toIndex)
    {
        Item source = originalContainer.Get(fromIndex);
        Item target = closestContainer.Get(toIndex);

        int maxStack = target.m_Details.MaxStack;

        int spaceLeft = maxStack - target.m_Quantity;

        if (spaceLeft <= 0)
            return 0;

        int moveAmount = Mathf.Min(spaceLeft, source.m_Quantity);

        target.m_Quantity += moveAmount;
        source.m_Quantity -= moveAmount;

        if (source.m_Quantity <= 0)
            originalContainer.Set(null, fromIndex);

        return moveAmount;
    }

    public void Swap(IContainer originalContainer, int fromIndex, IContainer closestContainer, int toIndex)
    {
        Item source = originalContainer.Get(fromIndex);
        Item target = closestContainer.Get(toIndex);

        originalContainer.Set(target, fromIndex);
        closestContainer.Set(source, toIndex);
    }

#endregion


#region Private methods
    
    private bool CanStack(Item a, Item b)
    {
        if (a == null || b == null)
            return false;

        if (a.m_Details.Id != b.m_Details.Id)
        {
            return false;
        }

        if (b.m_Details.MaxStack <= 1)
            return false;

        if (b.m_Quantity >= b.m_Details.MaxStack)
            return false;

        return true;
    }

    /// <summary>
    /// Creates and saves new container with created items.
    /// </summary>
    private Container BuildInventoryModelContainer(
        EntityManager em,
        string containerName,
        ItemManagedDatabase db,
        Entity containerEntity,
        DynamicBuffer<ContainerBuffer> buffer)
    {
        var container = new Container(containerEntity, containerName, buffer.Length);

        for (int i = 0; i < buffer.Length; i++)
        {
            var entry = buffer[i];
            var item = CreateItem(em, db, entry.ItemEntity, entry.Quantity);

            container.Set(item, i);
        }

        // Add to collection of containers
        return container;
    }

    /// <summary>
    /// Creates and saves new container with created items that can have own containers. 
    /// For example - weapon item can have own inner contaier
    /// </summary>
    private Container BuildInventoryModelContainerWithContainers(
        EntityManager em,
        string containerName,
        ItemManagedDatabase db,
        Entity containerEntity,
        DynamicBuffer<ContainerBuffer> buffer)
    {
        var container = new Container(containerEntity, containerName, buffer.Length);

        for (int i = 0; i < buffer.Length; i++)
        {
            var dataItem = buffer[i];
            var item = CreateItem(em, db, dataItem.ItemEntity, dataItem.Quantity);

            container.Set(item, i);            

            if (TryBuildWeaponContainer(em, db, dataItem.ItemEntity, i, out var weaponContainer))
            {
                if (weaponContainer != null && item is WeaponItem weapon)
                {
                    weapon.m_InnerContainer = weaponContainer;
                }
            }
        }

        // Save equipment container
        return container;
    }

    // WEAPON SUB-CONTAINERS

    private bool TryBuildWeaponContainer(
        EntityManager em,
        ItemManagedDatabase db,
        Entity ItemWithWeaponContainer,
        int index,
        out Container weaponContainer)
    {
        if (!em.HasComponent<WithWeaponContainer>(ItemWithWeaponContainer))
        {
            weaponContainer = null;
            return false;
        }

        var containerEntity = em.GetComponentData<WithWeaponContainer>(ItemWithWeaponContainer).Container;

        var weaponBuffer = em.GetBuffer<ContainerBuffer>(containerEntity);
        
        weaponContainer = new Container(containerEntity, "Weapon", weaponBuffer.Length);

        for (int i = 0; i < weaponBuffer.Length; i++)
        {
            var entry = weaponBuffer[i];
            var item = CreateItem(em, db, entry.ItemEntity, entry.Quantity);

            weaponContainer.Items.Set(item, i);
        }

        // Save weapon container
        SaveContainer(weaponContainer, SlotType.WeaponSlot);
        
        Debug.Log("[InventorySnapshotModel] WeaponContainer item has been added to WeaponContainers.");
        return true;
    }

    // Create item method. This one point where item can be created
    private Item CreateItem(
        EntityManager em,
        ItemManagedDatabase db,
        Entity itemEntity,
        int quantity)
    {
        if (itemEntity == Entity.Null)
            return null;

        if (!em.Exists(itemEntity))
        {
            Debug.LogWarning($"[InventorySnapshotModel] Entity does not exist: {itemEntity}");
            return null;
        }

        if (!em.HasComponent<CurrentItemId>(itemEntity))
        {
            Debug.LogWarning($"[InventorySnapshotModel] No CurrentItemId on {itemEntity}");
            return null;
        }

        var itemId = em.GetComponentData<CurrentItemId>(itemEntity).Value;
        var itemDetails = db.Get(itemId);

        if (em.HasComponent<WeaponTag>(itemEntity))
        {
            var level = em.GetComponentData<CurrentWeaponLevel>(itemEntity).Value;
            int capacity = em.GetComponentData<WeaponCapacity>(itemEntity).Value;
            bool shuffle = em.GetComponentData<WeaponShuffle>(itemEntity).Value;
            float spread = em.GetComponentData<WeaponSpread>(itemEntity).Value;
            float maxMana = em.GetComponentData<WeaponMaxMana>(itemEntity).Value;;
            float recoverRate = em.GetComponentData<WeaponManaRecoveryRate>(itemEntity).Value;
            float castDelay = em.GetComponentData<WeaponCastDelay>(itemEntity).Value;
            int castSpellNumber = em.GetComponentData<WeaponCastSpellNumber>(itemEntity).Value;
            
            return new WeaponItem(
                level: level,
                itemId: itemId,
                itemDetails: itemDetails, 
                quantity: quantity,
                capacity: capacity,
                manaRecoveryRate: recoverRate,
                maxMana: maxMana,
                spreadDegrees: spread,
                shuffle: shuffle,
                castDelay: castDelay,
                castSpellNumber: castSpellNumber
            );
        }
        else if (em.HasComponent<SpellTag>(itemEntity))
        {        
            return new SpellItem
            (
                itemId: itemId,
                itemDetails: itemDetails, 
                quantity: quantity
            );
        }
        else if (em.HasComponent<ConsumableTag>(itemEntity))
        {
            return new ConsumableItem
            (
                itemId: itemId,
                itemDetails: itemDetails, 
                quantity: quantity
            ); 
        }
        else 
        {
            Debug.LogWarning($"[InventorySnapshotModel] Current item doesn't have tag!");
            return null;
        }
    }

    /// <summary>
    /// Add to collection of containers
    /// </summary>
    private void SaveContainer(Container container, SlotType type)
    {
        _containers[new ContainerKey
        {
            Owner = container.OwnerEntity,
            Type = type,
        }] = container;
    }
}

#endregion