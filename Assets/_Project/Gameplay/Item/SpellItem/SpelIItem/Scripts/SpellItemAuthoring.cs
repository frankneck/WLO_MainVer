// using Unity.Entities;
// using Unity.NetCode;
// using UnityEngine;

// public class SpellAuthoring : MonoBehaviour
// {
//     [Header("Projectile linked the Spell Item")]
//     [SerializeField] private GameObject ProjectilePrefab;

//     class Baker : Baker<SpellAuthoring>
//     {
//         public override void Bake(SpellAuthoring authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
//             AddComponent<SpellTag>(entity);

//             AddComponent(entity, new ProjectileReference
//             {
//                 PrefabEntity = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic)
//             });
//         }
//     }
// }