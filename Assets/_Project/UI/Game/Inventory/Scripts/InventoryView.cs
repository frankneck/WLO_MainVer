using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class InventoryView : StorageView, IUIView 
{        
    [SerializeField] private VisualTreeAsset m_WeaponWindowTemplate;
    
    private Slot[] m_InventorySlots;
    private Slot[] m_WeaponEquipmentSlots;
    private Slot[] m_ConsumableEquipmentSlots;
    
    private EquipmentWeaponWindow[] m_WeaponWindows;

    public IReadOnlyList<Slot> InventorySlots => m_InventorySlots;
    public IReadOnlyList<Slot> WeaponEquipmentSlots => m_WeaponEquipmentSlots;
    public IReadOnlyList<Slot> ConsumableEquipmentSlots => m_ConsumableEquipmentSlots;
    public IReadOnlyList<EquipmentWeaponWindow> WeaponWindows => m_WeaponWindows;

    private int m_CurrentEquippedIndex = -1;
    
    // Initialization
    public override void Init(
        int inventoryCapacity, 
        int weaponEquippmentCapacity,
        int consumableEquipmentCapacity,
        int maxSlotsInWeapon, 
        ItemManagedDatabase itemManagedDatabase) 
    {
        m_Document = GetComponent<UIDocument>();

        // Slots
        m_InventorySlots = new Slot[inventoryCapacity];
        m_WeaponEquipmentSlots = new Slot[weaponEquippmentCapacity];
        m_ConsumableEquipmentSlots = new Slot[consumableEquipmentCapacity];
        
        // Weapon window from Equipment
        m_WeaponWindows = new EquipmentWeaponWindow[weaponEquippmentCapacity];

        m_Root = m_Document.rootVisualElement;

        // Container
        m_Container = m_Root.Q<VisualElement>("inventory__container");

        // Frames
        var weaponFrame = m_Root.Q<VisualElement>("inventory__weapon-frame");
        
        // Windows
        var equipmentWindow = m_Container.Q<VisualElement>("inventory__equipment-window");
        var backpackWindow = m_Container.Q<VisualElement>("inventory__backpack-window");
        
        // Slots containers
        var inventorySlots = backpackWindow.Q<VisualElement>("InventorySlots");
        var weaponEquipmentSlots = equipmentWindow.Q<VisualElement>("WeaponEquipmentSlots");
        var consumableEquipmentSlots = equipmentWindow.Q<VisualElement>("ConsumableEquipmentSlots");
        
        var weaponSlots = weaponFrame.Q<VisualElement>("WeaponSlots");
        
        // Creating slots
        ClearAndCreateSlots(inventorySlots, m_InventorySlots);
        ClearAndCreateSlots(weaponEquipmentSlots, m_WeaponEquipmentSlots);
        ClearAndCreateSlots(consumableEquipmentSlots, m_ConsumableEquipmentSlots);
        
        // Unite in one array - m_Slots
        var equpmentSlots = m_WeaponEquipmentSlots.Concat(m_ConsumableEquipmentSlots).ToArray();
        m_Slots = m_InventorySlots.Concat(equpmentSlots).ToArray();

        List<Slot> storage = new List<Slot>();

        // WeaponWindow from Weapon equipment
        weaponSlots.Clear();
        for (int i = 0; i < m_WeaponEquipmentSlots.Length; i++)
        {
            var weaponWindow = new EquipmentWeaponWindow(m_WeaponWindowTemplate, maxSlotsInWeapon);
            
            weaponSlots.Add(weaponWindow);
            m_WeaponWindows[i] = weaponWindow;
            storage.AddRange(m_WeaponWindows[i].Slots);
            
            // Default display
            weaponWindow.style.display = DisplayStyle.None;
        }
        // Unite in one array - m_Slots
        m_Slots = m_Slots.Concat(storage).ToArray();
        
        m_GhostIcon = m_Container.CreateChild("ghostIcon"); 
        
        // Create inventory entity
        // var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        // var inventoryEntity = em.CreateEntity();
        // em.AddComponentObject(inventoryEntity, new UIView
        // {
        //     m_Window = WindowType.Inventory,
        //     m_Root = m_Root
        // });

        RegisterDocument();
        
        // Drag&Drop
        RegisterCallbacks();

        PrepareUI();
    }

    public void Show()
    {
        m_Container.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_Container.style.display = DisplayStyle.None;
    }

    public void ShowSlot(SlotType type, int index)
    {
        switch (type)
        {
            case SlotType.InventorySlot :
                m_InventorySlots[index].style.display = DisplayStyle.Flex;
                break;
            case SlotType.WeaponEquipmentSlot :
                m_WeaponEquipmentSlots[index].style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void HideSlot(SlotType type, int index)
    {
        switch (type)
        {
            case SlotType.InventorySlot :
                m_InventorySlots[index].style.display = DisplayStyle.None;
                break;
            case SlotType.WeaponEquipmentSlot :
                m_WeaponEquipmentSlots[index].style.display = DisplayStyle.None;
                break;
        }
    }
    
    public void SetCurrentEquipped(int index)
    {
        // cashe
        if (index == m_CurrentEquippedIndex)
            return;

        if (index >= 0 &&
            index < m_WeaponWindows.Length)
        {
            m_WeaponWindows[index]
                .AddToClassList("weapon-window--selected");
        }

        m_CurrentEquippedIndex = index;
    }

    public void UnsetCurrentEquipped(int index)
    {
        if (m_CurrentEquippedIndex >= 0 && m_CurrentEquippedIndex < m_WeaponWindows.Length)
        {
            m_WeaponWindows[m_CurrentEquippedIndex].RemoveFromClassList("weapon-window--selected");
        }
    }

    private void RegisterDocument()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        var entity = em.CreateEntity();
        em.AddComponentObject(entity, this);
    }

    private void PrepareUI()
    {
        m_Container.style.display = DisplayStyle.None;
    }

    private void ClearAndCreateSlots(VisualElement UISlots, Slot[] slots)
    {
        UISlots.Clear();
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = UISlots.CreateChild<Slot>("slot");
            slots[i] = slot;
        }
    }
}