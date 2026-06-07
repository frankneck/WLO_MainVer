using Unity.Entities;
using UnityEngine;

public class SpellItemAuthoring : MonoBehaviour
{
    [SerializeField] private SpellItemDetails m_ItemDetails;

    class Baker : Baker<SpellItemAuthoring>
    {
        public override void Bake(SpellItemAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            
            // Tag
            AddComponent<SpellTag>(entity);
            
            // Common item data
            AddComponent<CurrentItemState>(entity);
            AddComponent<CurrentPickupMode>(entity);
            AddComponent(entity, new CurrentItemId { Value = authoring.m_ItemDetails.Id });
            
            // Special item data
            AddComponent(entity, new ProjectileReference
            {
                PrefabEntity = GetEntity(authoring.m_ItemDetails.ProjectilePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}