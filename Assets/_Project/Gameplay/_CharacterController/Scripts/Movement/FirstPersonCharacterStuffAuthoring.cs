using Unity.Entities;
using UnityEngine;

public class OffsetForSpellAuthoring : MonoBehaviour
{
    [SerializeField] private Transform Offset;
    
    public class Baker : Baker<OffsetForSpellAuthoring>
    {
        public override void Bake(OffsetForSpellAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.WorldSpace);
            AddComponent(entity, new OffsetForSpellSpawn { Value = authoring.Offset.position }); 
        }
    }
}
