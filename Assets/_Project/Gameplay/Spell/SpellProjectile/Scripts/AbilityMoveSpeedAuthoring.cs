using Unity.Entities;
using UnityEngine;

public class AbilityMoveSpeedAuthoring : MonoBehaviour
{
    public float AbilityMoveSpeed;

    class AbilityBaker : Baker<AbilityMoveSpeedAuthoring>
    {
        public override void Bake(AbilityMoveSpeedAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ProjectileMoveSpeed { Value = authoring.AbilityMoveSpeed });
            AddComponent<SpellDirection>(entity);   
        }
    }
}