using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterBodyColorAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterBodyColorAuthoring>
    {
        public override void Bake(CharacterBodyColorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable | TransformUsageFlags.None);
            
            AddComponent<URPMaterialPropertyBaseColor>(entity);
            SetComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(0, 0, 0, 0) });
        }
    }
}