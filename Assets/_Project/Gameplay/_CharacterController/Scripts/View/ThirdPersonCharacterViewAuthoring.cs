using Unity.Entities;
using UnityEngine;

class PlayerCharacterViewAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject ThirdPersonViewPrefab; 

    class ThirdPersonCharacterViewAuthoringBaker : Baker<PlayerCharacterViewAuthoring>
    {
        public override void Bake(PlayerCharacterViewAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<LastPlayerCharacterView>(entity);
            AddComponent(entity, new PlayerCharacterViews
            {
                TPView = GetEntity(authoring.ThirdPersonViewPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

