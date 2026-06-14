using Unity.Entities;
using UnityEngine;

public class AbilityAoeTrapAuthoring : MonoBehaviour
{
    [Header("Jelly Zone Settings")]
    public float speedMultiplier = 0.35f;
    public float sharpnessMultiplier = 0.65f;
    public float airAccelerationMultiplier = 0.4f;
    public float airMaxSpeedMultiplier = 0.5f;
    public float airDragMultiplier = 2.2f;
    public float gravityMultiplier = 0.9f;
    public float jumpMultiplier = 0.8f;

    class AbilityBaker : Baker<AbilityAoeTrapAuthoring>
    {
        public override void Bake(AbilityAoeTrapAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(entity, new JellyZone
            {
                SpeedMultiplier = authoring.speedMultiplier,
                SharpnessMultiplier = authoring.sharpnessMultiplier,
                AirAccelerationMultiplier = authoring.airAccelerationMultiplier,
                AirMaxSpeedMultiplier = authoring.airMaxSpeedMultiplier,
                AirDragMultiplier = authoring.airDragMultiplier,
                GravityMultiplier = authoring.gravityMultiplier,
                JumpMultiplier = authoring.jumpMultiplier
            });
        }
    }
}