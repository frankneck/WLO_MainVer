using Unity.Entities;
using UnityEngine;

public class MatchControlledAuthoring : MonoBehaviour
{
    class Baker : Baker<MatchControlledAuthoring>
    {
        public override void Bake(MatchControlledAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<MatchControlledTag>(entity);
        }
    }
}