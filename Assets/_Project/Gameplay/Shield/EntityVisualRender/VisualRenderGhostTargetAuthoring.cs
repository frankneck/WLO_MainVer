// using Unity.Entities;
// using UnityEngine;

// public class VisualRenderGhostTargetAuthoring : MonoBehaviour
// {
//     class GhostTargetBaker : Baker<VisualRenderGhostTargetAuthoring>
//     {
//         public override void Bake(VisualRenderGhostTargetAuthoring authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
//             AddComponent<VisualRenderGhostTarget>(entity);
//         }
//     }
// }