using Unity.Entities;
using UnityEngine;

public class PlayerInitAuthoring : MonoBehaviour
{   
    public GameObject CharacterPrefab;
    public GameObject PlayerPrefab;

    class Baker : Baker<PlayerInitAuthoring>
    {
        public override void Bake(PlayerInitAuthoring authoring)
        {
            AddComponent(GetEntity(authoring, TransformUsageFlags.None), new GhostPrefabs
            {
                CharacterPrefab = GetEntity(authoring.CharacterPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.None),
            });
        }
    }
}