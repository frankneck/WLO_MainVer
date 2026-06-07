using UnityEngine;
using Unity.Entities;

/// <summary>
/// Adds main component for container
/// </summary>
public class ContainerAuthoring : MonoBehaviour
{
    public class Baker : Baker<ContainerAuthoring>
    {
        public override void Bake(ContainerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent<ContainerTag>(entity);
            AddComponent<ContainerVersion>(entity);
            AddBuffer<ContainerBuffer>(entity);
            AddComponent<ContainerTypeComponent>(entity);
        }
    }
}