// using Unity.Entities;
// using Unity.NetCode;
// using UnityEngine;

// public class PredictedShieldAuthoring : MonoBehaviour
// {
//     class ItemBaker : Baker<PredictedShieldAuthoring>
//     {
//         public override void Bake(PredictedShieldAuthoring authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
//             AddComponent<PredictedShieldTag>(entity);
//             AddComponent<ShieldState>(entity);
//         }
//     }
// }