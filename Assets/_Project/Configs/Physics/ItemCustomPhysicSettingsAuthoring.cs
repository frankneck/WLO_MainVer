using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ItemCustomPhysicSettingsAuthoring : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] private float m_LinearThreshold;
    [Range(0, 10)]
    [SerializeField] private float m_AngularThreshold;
    [Range(0, 10)]
    [SerializeField] private float m_TimeToSleep;

    class Baker : Baker<ItemCustomPhysicSettingsAuthoring>
    {
        public override void Bake(ItemCustomPhysicSettingsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            AddComponent(entity, new CustomItemPhysicsSettings
            {
                LinearThresholdSq = math.pow(authoring.m_LinearThreshold, 2),
                AngularThresholdSq = math.pow(authoring.m_AngularThreshold, 2)
            });
        }
    }
}
