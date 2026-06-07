using Unity.Entities;
using UnityEngine;

class RoundAuthoring : MonoBehaviour
{
    class RoundAuthoringBaker : Baker<RoundAuthoring>
    {
        public override void Bake(RoundAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent<BelongsToMatch>(entity);           
            AddComponent<RoundTag>(entity);            
        }
    }
}

