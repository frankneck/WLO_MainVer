using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;


public class SceneLoadingAuthoring : MonoBehaviour
{
    class Baker : Baker<SceneLoadingAuthoring>
    {
        public override void Bake(SceneLoadingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<EntitySceneReferenceBufferElementData>(entity);
        }
    }
}