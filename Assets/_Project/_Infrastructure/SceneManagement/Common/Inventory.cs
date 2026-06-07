using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [SerializeField] private InventoryController m_InventoryController; 
    [SerializeField] private InventoryView m_InventoryView; 

    public void Init(
        InventoryConfig inventoryConfig,
        ItemManagedDatabase itemManagedDatabase)
    {
        BindObjects();
        InitializeObjects(inventoryConfig, itemManagedDatabase);
    }

    public InventoryView GetInventoryView() => m_InventoryView;

    private void BindObjects()
    {
        m_InventoryView = Instantiate(m_InventoryView);
        m_InventoryController = Instantiate(m_InventoryController);
    }

    private void InitializeObjects(
        InventoryConfig config, 
        ItemManagedDatabase itemManagedDatabase)
    {
        m_InventoryView.Init( 
            config.InventoryMaxCapacity, 
            config.WeaponEquipmentMaxCapacity, 
            config.ConsumableEquipmentMaxCapacity,
            config.WeaponMaxCapacity, 
            itemManagedDatabase
        );

        m_InventoryController.Init(
            m_InventoryView, 
            itemManagedDatabase
        );
    }
}