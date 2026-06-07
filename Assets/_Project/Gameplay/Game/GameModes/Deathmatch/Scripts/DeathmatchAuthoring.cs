using Unity.Entities;
using UnityEngine;

public class DeathmatchAuthoring : MonoBehaviour
{
    class Baker : Baker<DeathmatchAuthoring>
    {
        public override void Bake(DeathmatchAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<DeathmatchTeamsData>(entity);

            AddComponent<MatchIdComponent>(entity);
            AddComponent<MatchTag>(entity);
            
            AddComponent<DeathmatchMatchTag>(entity);
            AddComponent<DeathmatchMatchSettings>(entity);

            AddComponent<PlayedRoundsNumber>(entity);

            AddComponent<RoundTimer>(entity);           
            AddComponent<LeftSecondsToFinishRoundTimer>(entity);  
        }
    }
}