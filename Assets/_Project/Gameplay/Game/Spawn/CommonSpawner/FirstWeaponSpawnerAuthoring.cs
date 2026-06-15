using Unity.Entities;
using UnityEngine;

public class FirstWeaponsSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject m_CollectableItem;

    class Baker : Baker<FirstWeaponsSpawnerAuthoring>
    {
        public override void Bake(FirstWeaponsSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            
            // identifacl component
            AddComponent<SpawnerTag>(entity);

            AddComponent<FirstWeaponsSpawnerTag>(entity);
            
            AddComponent(entity, new FirstWeaponsQuantity
            {
                Value = 1
            });

            // target prefab
            AddComponent(entity, new SpawnTargetEntity
            {
                Entity = GetEntity(authoring.m_CollectableItem, TransformUsageFlags.Dynamic)
            });
        }
    }
}