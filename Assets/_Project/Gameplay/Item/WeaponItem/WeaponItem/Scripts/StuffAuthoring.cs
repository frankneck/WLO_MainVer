// using Unity.Entities;
// using UnityEngine;

// public class WeaponItemAuthoring : MonoBehaviour
// {
//     class Baker : Baker<WeaponItemAuthoring>
//     {
//         public override void Bake(WeaponItemAuthoring authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
//             AddComponent<WeaponTag>(entity);

//             // Starndart properties
//             AddComponent<WeaponSpread>(entity);
//             AddComponent<WeaponShuffle>(entity);
//             AddComponent<WeaponCastSpellNumber>(entity);

//             // Cast Delay
//             AddComponent<WeaponCastDelay>(entity);
//             AddBuffer<WeaponCastDelayTargetTicks>(entity);
            
//             // Random sequance of casting spells
//             AddComponent<StuffSpellState>(entity);
//             AddComponent<NeedsRandomInit>(entity);
            
//             // Inventory
//             AddBuffer<SpellsInWeaponBuffer>(entity);
//             AddComponent<InitWeapon>(entity);
//             AddComponent<WeaponCapacity>(entity);
//         }
//     }
// }