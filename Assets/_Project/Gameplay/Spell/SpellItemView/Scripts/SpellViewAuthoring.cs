using Unity.Entities;
using UnityEngine;

class SpellViewAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject WorldViewPrefab; 

    class Baker : Baker<SpellViewAuthoring>
    {
        public override void Bake(SpellViewAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<LastViewEntity>(entity);
            AddComponent<EquipedBy>(entity);
            AddComponent(entity, new ItemViews
            {
                WorldViewPrefab = GetEntity(authoring.WorldViewPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

