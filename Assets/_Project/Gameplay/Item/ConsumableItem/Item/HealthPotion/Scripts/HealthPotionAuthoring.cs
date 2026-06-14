using Unity.Entities;
using UnityEngine;

class HealthPotionAuthoring : MonoBehaviour
{
    public int HealthPotionVolume;
}

class HealthPotionAuthoringBaker : Baker<HealthPotionAuthoring>
{
    public override void Bake(HealthPotionAuthoring authoring)
    {
        var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
        
        AddComponent<ItemTag>(entity);
        AddComponent<HealthPotionTag>(entity);
        AddComponent(entity, new HealthPotionVolume
        {
            Value = authoring.HealthPotionVolume 
        });
    }
}
