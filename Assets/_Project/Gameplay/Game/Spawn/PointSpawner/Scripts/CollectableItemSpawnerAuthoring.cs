using UnityEngine;
using Unity.Entities;
using UnityEditor;
using Unity.NetCode;

public class CollectableItemSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject m_CollectableItem;
    [SerializeField] private Transform m_SpawnPointTransform; 
    [SerializeField] private float m_TimeCooldownAfterPickuping;
    [SerializeField] private SpawnerMode m_SpawnerMode;
    
    private Vector3 _gizmosCubeSize = new Vector3(0.1f, 0.1f, 0.1f); 

    class Baker : Baker<CollectableItemSpawnerAuthoring>
    {
        public override void Bake(CollectableItemSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // identifacl component
            AddComponent<SpawnerTag>(entity);
            
            // current spawner state
            AddComponent(entity, new CurrentSpawnerState 
            { 
                Value = SpawnerState.Active 
            });
            
            AddComponent(entity, new CurrentSpawnerMode 
            { 
                Value = authoring.m_SpawnerMode  
            });
            
            // parameters
            AddComponent<SpawnerTargetTick>(entity);
            AddComponent(entity, new SpawnerCooldown 
            { 
                Value = authoring.m_TimeCooldownAfterPickuping 
            });
            
            // target prefab
            AddComponent(entity, new SpawnTargetEntity
            {
                Entity = GetEntity(authoring.m_CollectableItem, TransformUsageFlags.Dynamic)
            });
            
            // where spawn
            AddComponent(entity, new SpawnPointTransform
            { 
                Position = authoring.m_SpawnPointTransform.position,
                Rotation = authoring.m_SpawnPointTransform.rotation 
            });
            
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(m_SpawnPointTransform.position, _gizmosCubeSize);
    }
}