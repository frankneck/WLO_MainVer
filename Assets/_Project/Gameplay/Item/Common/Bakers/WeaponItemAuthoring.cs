using Unity.Entities;
using UnityEngine;

public class WeaponItemAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponItemDetails m_ItemDetails;

    class ItemBaker : Baker<WeaponItemAuthoring>
    {
        public override void Bake(WeaponItemAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            // Tag
            AddComponent<ItemTag>(entity);
            AddComponent<WeaponTag>(entity);
            
            // Common item data
            AddComponent<CurrentItemState>(entity);
            AddComponent<CurrentPickupMode>(entity);
            
            AddComponent(entity, new CurrentItemId 
            { 
                Value = authoring.m_ItemDetails.Id 
            });

            // Special item data
            AddComponent<ItemControl>(entity);

            // Starndart properties
            AddComponent<WeaponSpread>(entity);
            AddComponent<WeaponShuffle>(entity);
            AddComponent<WeaponCastSpellNumber>(entity);

            // Cast Delay
            AddComponent<WeaponCastDelay>(entity);
            AddBuffer<WeaponCastDelayTargetTicks>(entity);
            
            // Random sequance of casting spells
            AddComponent<StuffSpellState>(entity);
            AddComponent<NeedsRandomInit>(entity);
            
            // Weapon container
            AddComponent<WeaponCapacity>(entity);
            AddComponent<WithWeaponContainer>(entity);
            AddComponent<NeedToCreateContainer>(entity);
            SetComponentEnabled<NeedToCreateContainer>(entity, false);
        }
    }
}