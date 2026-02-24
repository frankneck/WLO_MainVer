using Unity.Entities;
using UnityEngine;

public class HitPointsAuthoring : MonoBehaviour
{
    public int MaxHitPoints;

    class HitPointsBaker : Baker<HitPointsAuthoring>
    {
        public override void Bake(HitPointsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new MaxHitPoints { Value = authoring.MaxHitPoints });
            AddComponent(entity, new CurrentHitPoints { Value = authoring.MaxHitPoints });
            AddBuffer<DamageBufferElement>(entity);
            AddBuffer<DamageThisTick>(entity);
        }
    }
}