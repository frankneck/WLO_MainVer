using Unity.Entities;
using UnityEngine;

public class SpellItemAuthoring : MonoBehaviour
{
    [SerializeField] private SpellItemDetails m_Config;

    class Baker : Baker<SpellItemAuthoring>
    {
        public override void Bake(SpellItemAuthoring authoring)
        {
            var config = authoring.m_Config;

            var entity = GetEntity(authoring, TransformUsageFlags.None);
            
            // Tag
            AddComponent<ItemTag>(entity);
            AddComponent<SpellTag>(entity);
            
            // COMMON ITEM DATA

            AddComponent<CurrentItemState>(entity);
            AddComponent<CurrentPickupMode>(entity);
            AddComponent(entity, new CurrentItemId { Value = config.Id });
            
            // SPECIAL ITEM DATA

            AddComponent(entity, new ProjectileEntityReference
            {
                PrefabEntity = GetEntity(config.ProjectilePrefab, TransformUsageFlags.Dynamic)
            });
            
            AddComponent(entity, new ManaCost 
            { 
                Value = config.ManaCost
            });

            AddComponent(entity, new SpellTypeComponent 
            { 
                Value = config.SpellType 
            });

            AddComponent(entity, new SpellDistance 
            { 
                Value = config.Distance 
            });
        }
    }
}