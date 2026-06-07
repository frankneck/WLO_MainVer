using Unity.Entities;
using UnityEngine;

public class PlayerInitAuthoring : MonoBehaviour
{   
    [SerializeField] private GameObject CharacterPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject RespawnEntity;
    [SerializeField] private GameObject ItemContainerPrefab;
    [SerializeField] private GameObject PlayerTeamPrefab;

    class Baker : Baker<PlayerInitAuthoring>
    {
        public override void Bake(PlayerInitAuthoring authoring)
        {
            AddComponent(GetEntity(authoring, TransformUsageFlags.None), new GhostPrefabs
            {
                CharacterPrefab = GetEntity(authoring.CharacterPrefab, TransformUsageFlags.None),
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.None),
                RespawnEntity = GetEntity(authoring.RespawnEntity, TransformUsageFlags.None),
                ItemContainer = GetEntity(authoring.ItemContainerPrefab, TransformUsageFlags.None),
                PlayerTeam = GetEntity(authoring.PlayerTeamPrefab, TransformUsageFlags.None)
            });
        }
    }
}