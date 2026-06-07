using Unity.Entities;
using UnityEngine;

public class ShieldVisualPrefabAuthoring : MonoBehaviour
{
    public GameObject ShieldVisualPrefab;

    class VisualPrefabsBaker : Baker<ShieldVisualPrefabAuthoring>
    {
        public override void Bake(ShieldVisualPrefabAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new VisualPrefabs { 
                Shield = GetEntity(authoring.ShieldVisualPrefab, TransformUsageFlags.Dynamic
            )});
        }
    }
}
