using UnityEngine;
using Unity.Entities;

public class ShieldPrefabAuthoring : MonoBehaviour
{
    public GameObject ShieldPrefab;

    class Baker : Baker<ShieldPrefabAuthoring>
    {
        public override void Bake(ShieldPrefabAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new ShieldPrefab
            {
                Shield = GetEntity(authoring.ShieldPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}
