using Unity.Entities;
using UnityEngine;

public class HitPointsAuthoring : MonoBehaviour
{
    public float MaxHitPoints;
    public float HealthRegeneratioSpeed;

    class HitPointsBaker : Baker<HitPointsAuthoring>
    {
        public override void Bake(HitPointsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new MaxHealth { Value = authoring.MaxHitPoints });
            AddComponent(entity, new CurrentHealth { Value = authoring.MaxHitPoints });
            AddComponent(entity, new HealthRegenerationSpeed { Value = authoring.HealthRegeneratioSpeed }); 
            AddComponent(entity, new RegenerationHealthAccumulated());
            AddBuffer<DamageBufferElement>(entity);
            AddBuffer<DamageThisTick>(entity);
        }
    }
}