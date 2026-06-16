using Unity.Entities;
using UnityEngine;

public class PlayerFirstWeaponsSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private InventoryConfig m_InventoryConfig;
    [SerializeField] [Range(1, 10)] private int m_Quantity;
    [SerializeField] private GameObject m_WeaponItemPrefab;

    private void OnValidate()
    {
        if (m_InventoryConfig == null)
        {
            Debug.LogError($"Error: player first weapons spawner hasn't been fully set. Set m_InventoryConfig.");
            return;
        }

        if (m_Quantity > m_InventoryConfig.WeaponEquipmentMaxCapacity)
        {
            Debug.LogWarning($"Attention: player first weapons spawner quantity [{m_Quantity}] is more than m_InventoryConfig.MaxWeaponEquipmentCapacity [{m_InventoryConfig.WeaponEquipmentMaxCapacity}]. m_Quantity has been changed on m_InventoryConfig.MaxWeaponEquipmentCapacity");
            m_Quantity = m_InventoryConfig.WeaponEquipmentMaxCapacity;
        }
    }

    class Baker : Baker<PlayerFirstWeaponsSpawnerAuthoring>
    {
        public override void Bake(PlayerFirstWeaponsSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            
            // identifacl component
            AddComponent<SpawnerTag>(entity);

            AddComponent<PlayerFirstWeaponsSpawnerTag>(entity);
            
            AddComponent(entity, new PlayerFirstWeaponsSpawnerQuantity
            {
                Value = authoring.m_Quantity
            });

            // target prefab
            AddComponent(entity, new SpawnerTargetEntity
            {
                PrefabEntity = GetEntity(authoring.m_WeaponItemPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}