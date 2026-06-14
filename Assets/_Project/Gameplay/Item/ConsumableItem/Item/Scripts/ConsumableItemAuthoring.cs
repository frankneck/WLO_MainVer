using Unity.Entities;
using UnityEngine;

public class ConsumableItemAuthoring : MonoBehaviour
{
    [SerializeField] private ConsumableItemDetails m_ItemDetails;

    class ItemBaker : Baker<ConsumableItemAuthoring>
    {
        public override void Bake(ConsumableItemAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            // Tag
            AddComponent<ConsumableTag>(entity);
            
            // Common item data
            AddComponent<CurrentItemState>(entity);
            AddComponent<CurrentPickupMode>(entity);
            AddComponent(entity, new CurrentItemId 
            { 
                Value = authoring.m_ItemDetails.Id 
            });

            AddComponent<ItemControl>(entity);

            // Special item data
            AddComponent(entity, new ConsumableTypeComponent
            {
                Value = authoring.m_ItemDetails.ConsumableType
            });
        }
    }
}