// using Unity.Entities;
// using UnityEngine;

// public class ItemAuthoring : MonoBehaviour
// {
//     [SerializeField] private ItemDetails itemDetails;

//     class ItemBaker : Baker<ItemAuthoring>
//     {
//         public override void Bake(ItemAuthoring authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.None);
            
//             AddComponent<CurrentItemState>(entity);
            
//             AddComponent(entity, new CurrentPickupMode
//             {   
//                 Mode = authoring.itemDetails.PickupMode
//             });
            
//             AddComponent(entity, new CurrentItemId 
//             { 
//                 Value = authoring.itemDetails.Id
//             });
//         }
//     }
// }