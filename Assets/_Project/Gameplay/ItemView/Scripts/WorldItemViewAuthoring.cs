using Unity.Entities;
using UnityEngine;

class WorldItemViewAuthoring : MonoBehaviour
{
    class WeaponViewAuthoringBaker : Baker<WorldItemViewAuthoring>
    {
        public override void Bake(WorldItemViewAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<ItemViewTransform>(entity);
        }
    }
}
