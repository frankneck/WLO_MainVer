using Unity.Entities;
using UnityEngine;

class EquipableItemViewAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject WorldViewPrefab; 
    [SerializeField] private GameObject FirstPersonViewPrefab; 
    [SerializeField] private GameObject ThirdPersonViewPrefab; 

    class WeaponViewAuthoringBaker : Baker<EquipableItemViewAuthoring>
    {
        public override void Bake(EquipableItemViewAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<LastViewEntity>(entity);
            AddComponent<EquipedBy>(entity);
            AddComponent(entity, new ItemViews
            {
                WorldViewPrefab = GetEntity(authoring.WorldViewPrefab, TransformUsageFlags.Dynamic),
                FirstPersonViewPrefab =  GetEntity(authoring.FirstPersonViewPrefab, TransformUsageFlags.Dynamic),
                ThirdPersonViewPrefab =  GetEntity(authoring.ThirdPersonViewPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

