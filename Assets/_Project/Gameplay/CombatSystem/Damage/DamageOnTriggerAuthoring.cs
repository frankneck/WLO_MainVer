using Unity.Entities;
using UnityEngine;

public class DamageOnTriggerAuthoring : MonoBehaviour
{
    public int DamageOnTrigger;

    class DamageOnTriggerBaker : Baker<DamageOnTriggerAuthoring>
    {
        public override void Bake(DamageOnTriggerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new DamageOnTrigger { Value = authoring.DamageOnTrigger });
            AddBuffer<AlreadyDamagedEntity>(entity);
        }
    }
}