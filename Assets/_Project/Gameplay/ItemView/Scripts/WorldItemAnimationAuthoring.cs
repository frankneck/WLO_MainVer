using Unity.Entities;
using UnityEngine;

public class WorldItemAnimationAuthoring : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] private float Scale;
    [Range(0, 1)]
    [SerializeField] private float RotationSpeed;
    [Range(0, 1)]
    [SerializeField] private float Amplitude;

    class Baker : Baker<WorldItemAnimationAuthoring>
    {
        public override void Bake(WorldItemAnimationAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new WorldViewAnimaParameters
            {
                Scale = authoring.Scale,
                Amplitude = authoring.Amplitude,
                RotationSpeed = authoring.RotationSpeed,
                Initialized = false
            });
        }
    }
}