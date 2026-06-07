using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Scriptable Objects/InventoryConfig")]
public class InventoryConfig : ScriptableObject
{
    [SerializeField] private int m_InventoryMaxCapacity;
    [SerializeField] private int m_WeaponEquipmentMaxCapacity;
    [SerializeField] private int m_ConsumableEquipmentMaxCapacity;
    [SerializeField] private int m_WeaponMaxCapacity;

    public int InventoryMaxCapacity => m_InventoryMaxCapacity;
    public int WeaponEquipmentMaxCapacity => m_WeaponEquipmentMaxCapacity;
    public int ConsumableEquipmentMaxCapacity => m_ConsumableEquipmentMaxCapacity;
    public int WeaponMaxCapacity => m_WeaponMaxCapacity;
}  