using Unity.Entities;
using UnityEngine;

public class GameMatchGlobalSettingsAuthoring : MonoBehaviour
{
    [SerializeField] private GameMatchGlobalSettingsConfig m_Config;

    public class Baker : Baker<GameMatchGlobalSettingsAuthoring>
    {
        public override void Bake(GameMatchGlobalSettingsAuthoring authoring)
        {
            var configData = authoring.m_Config.GetConfigData();

            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent<GameMatchGlobalSettingsTag>(entity);

            AddComponent(entity, new GameMatchGlobalSettings
            {
                MinPlayersToStartMatch = configData.MinPlayersToStartDominationMatch,
                MinPlayersPerTeamToStartDeathmatch = configData.MinPlayersPerTeamToStartMatch,
                TimeBeforeStartingRound = configData.TimeBeforeStartingRound,
                TimeAfterFinishingMatch = configData.TimeAfterFinishingMatch,
                TimeAfterFinishingRound = configData.TimeAfterFinishingRound,
            });
        }
    }
}