using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    private InventorySnapshotModel m_Model;
    private ItemManagedDatabase m_ItemManagedDatabase;
    private InventoryView m_View;

    private InventoryCommandBuffer m_CommandBuffer;
    public InventoryCommandBuffer CommandBuffer => m_CommandBuffer;

    public void Init(InventoryView view, ItemManagedDatabase itemManagedDatabase)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        m_View = view;
        m_ItemManagedDatabase = itemManagedDatabase; 
        m_CommandBuffer = new();
    }

    // Constructor

#region Public methods

    /// <summary>
    /// Creates and return ready snapshot model for UI
    /// </summary>
    public void CreateInventorySnapshotModel(
        EntityManager em,
        Entity inventoryContainer,
        Entity weaponEquipmentContainer,
        Entity consumableEquipmentContainer,
        DynamicBuffer<ContainerBuffer> inventoryBuffer,
        DynamicBuffer<ContainerBuffer> weaponEquipmentBuffer,
        DynamicBuffer<ContainerBuffer> consumableContainerBuffer 
    )
    {   
        m_Model = new InventorySnapshotModel(
            ref em,
            m_ItemManagedDatabase,
            inventoryContainer: inventoryContainer,
            weaponEquipmentContainer: weaponEquipmentContainer,
            consumableEquipmentContainer: consumableEquipmentContainer,
            inventoryBuffer: inventoryBuffer,
            weaponEquipmentBuffer: weaponEquipmentBuffer,
            consumableEquipmentBuffer: consumableContainerBuffer
        );

        m_Model.InventoryContainer.OnModelContainerChanged += HandleContainerChanged;
        m_Model.WeaponEquipmentContainer.OnModelContainerChanged += HandleContainerChanged;
        m_Model.ConsumableEquipmentContainer.OnModelContainerChanged += HandleContainerChanged;

        m_View.OnDropEvent += HandleDrop;

        InitEquipmentAndInventorySlots();
        UpdateEquipmentAndInventorySlotsData();
        
        RebuildWeaponWindows();

        UnityEngine.Debug.Log("[Inventory] Created new inventory snapshotmodel");
    }

    public void DestroyInventorySnapshoModel()
    {
        UnityEngine.Debug.Log("[Inventory] DestroyInventorySnapshoModel");

        if (m_Model != null)
        {
            m_Model.InventoryContainer.OnModelContainerChanged -= HandleContainerChanged;
            m_Model.WeaponEquipmentContainer.OnModelContainerChanged -= HandleContainerChanged;
            m_Model.ConsumableEquipmentContainer.OnModelContainerChanged -= HandleContainerChanged;
            
            m_View.OnDropEvent -= HandleDrop;
            
            m_Model.WeaponEquipmentContainer = null;
            m_Model.ConsumableEquipmentContainer = null;
            m_Model.InventoryContainer = null;

            m_Model = null;
            m_CommandBuffer.Clear();
            m_View.Clear();
        }
    }


#endregion

#region Private methods

    private void HandleDrop(Slot originalSlot, Slot closestSlot)
    {        
        if (m_Model.TryMove(originalSlot, closestSlot))
        {
            // If something changes add command
            AddInventoryCommand(originalSlot, closestSlot);
        }

        UpdateEquipmentAndInventorySlotsData();
        RebuildWeaponWindows();
    }
    
    private void AddInventoryCommand(Slot orignalSlot, Slot closestSlot)
    {
        var command = new InventoryCommand
        {
            SourceOwner = orignalSlot.m_EntityOwner,  
            SourceType = orignalSlot.m_Type,
            SourceIndex = orignalSlot.m_Index,
            TargetOwner = closestSlot.m_EntityOwner,  
            TargetType = closestSlot.m_Type,
            TargetIndex = closestSlot.m_Index,
        };

        m_CommandBuffer.Add(command);
    } 

    // Update slots in inventory
    private void HandleContainerChanged(IList<Item> items) 
    {
        UpdateEquipmentAndInventorySlotsData();
        RebuildWeaponWindows();
    }
    
    /// <summary>
    /// Inits all slots
    /// </summary>
    private void InitEquipmentAndInventorySlots()
    {
        InitSlots(m_View.InventorySlots, SlotType.InventorySlot);
        InitSlots(m_View.WeaponEquipmentSlots, SlotType.WeaponEquipmentSlot);
        InitSlots(m_View.ConsumableEquipmentSlots, SlotType.ConsumableEquipmentSlot);
    }

    private void InitSlots(IReadOnlyList<Slot> slots, SlotType slotType)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Init(i, slotType);
        }
    }

    /// <summary>
    /// Updates all slots
    /// </summary>
    private void UpdateEquipmentAndInventorySlotsData()
    {        
        var inventoryContainer = m_Model.InventoryContainer;
        var equipmentContainer = m_Model.WeaponEquipmentContainer;
        var consumableContainer = m_Model.ConsumableEquipmentContainer;

        UpdateSlots(m_View.InventorySlots, inventoryContainer);
        UpdateSlots(m_View.WeaponEquipmentSlots, equipmentContainer);
        UpdateSlots(m_View.ConsumableEquipmentSlots, consumableContainer);
    }

    private void UpdateSlots(IReadOnlyList<Slot> slots, Container container)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= container.Items.Capacity)
                break;

            var slot = slots[i];
            Item item = container.Items[i];

            var data = new SlotViewData
            {
                Owner = container.OwnerEntity,
                ItemId = item?.m_ItemId ?? ItemId.Empty,
                Sprite = item?.m_Details.Sprite,
                Quantity = item?.m_Quantity ?? 0,
            };
            
            slot.Set(data, SlotState.Active);

            // if current item is null remove slot level color
            if (item == null)
                continue;

            // if item is weapon get weapon level (only weapon has level in cur.ver.)
            WeaponLevel level = item is WeaponItem weapon ? weapon.m_Level : WeaponLevel.None;
            slot.SetColor(level);
        }
    }

    // Rebuild fully weapon windows
    private void RebuildWeaponWindows()
    {
        for (int i = 0; i < m_View.WeaponWindows.Count; i++)
        {
            var equipmentContainer = m_Model.WeaponEquipmentContainer;

            var item = equipmentContainer.Get(i) as WeaponItem;

            if (item == null || item.m_InnerContainer == null)
            {
                m_View.WeaponWindows[i].Hide();
                continue;
            }

            var data = new EquipmentWindowViewData
            {
                Capacity = item.m_WeaponCapacity,
                Spread = item.m_Spread,
                CastDelay = item.m_CastDelay,
                Shuffle = item.m_Shuffle,
                MaxMana = item.m_MaxMana,
                RecoveryRate = item.m_RecoveryRate,
                CastSpellNumber = item.m_CastSpellNumber,

                Container = item.m_InnerContainer,
                ItemdId = item.m_ItemId,
                Sprite = item.m_Details.Sprite
            };

            m_View.WeaponWindows[i].Set(data);
        }
    }
}

#endregion

public struct EquipmentWindowViewData
{
    public ItemId ItemdId;
    public Container Container;
    public Sprite Sprite;

    public int Capacity;
    public int CastSpellNumber;
    public float RecoveryRate;
    public float MaxMana;
    public float Spread;
    public bool Shuffle; 
    public float CastDelay; 
}
