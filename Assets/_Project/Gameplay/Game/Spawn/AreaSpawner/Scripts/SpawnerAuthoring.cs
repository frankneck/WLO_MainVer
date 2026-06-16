using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject SpawningPrefab;

    [SerializeField] private int NumberOfEntitiesToSpawn;

    [Range(0, 100)]
    [SerializeField] private float SpawnRadius;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<SpawnerTag>(entity);
            AddComponent(entity, new NumberEntitiesToSpawn { Value = authoring.NumberOfEntitiesToSpawn });
            AddComponent(entity, new SpawnRadius { Value = authoring.SpawnRadius });
            AddComponent<RadiusRandom>(entity);
            AddComponent(entity, new SpawnerTargetEntity { PrefabEntity = GetEntity(authoring.SpawningPrefab, TransformUsageFlags.None )});
            AddComponent(entity, new CurrentSpawnerState { Value = SpawnerState.Active });
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, SpawnRadius);
    }
}
