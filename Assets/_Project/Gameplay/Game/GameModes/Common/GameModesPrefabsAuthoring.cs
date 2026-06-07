using Unity.Entities;
using UnityEngine;

public class GameModesPrefabsAuthoring : MonoBehaviour
{
    [Header("Game modes")]
    [SerializeField] private GameObject m_DeathracePrefab;  
    [SerializeField] private GameObject m_DeathmatchPrefab;

    [Header("Round")]
    [SerializeField] private GameObject m_RoundPrefab;

    class Baker : Baker<GameModesPrefabsAuthoring>
    {
        public override void Bake(GameModesPrefabsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new GameModesPrefabs
            {
                Deathmatch = GetEntity(authoring.m_DeathmatchPrefab, TransformUsageFlags.None),
                Deathrace = GetEntity(authoring.m_DeathracePrefab, TransformUsageFlags.None),
                RoundEntityPrefab = GetEntity(authoring.m_RoundPrefab, TransformUsageFlags.None)
            });
        }
    }
}