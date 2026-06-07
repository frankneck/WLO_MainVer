using Unity.Entities;
using UnityEngine;

public class GameModeDominationAuthoring : MonoBehaviour
{
    class Baker : Baker<GameModeDominationAuthoring>
    {
        public override void Bake(GameModeDominationAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<MatchIdComponent>(entity);
            AddComponent<MatchTag>(entity);
            
            AddComponent<DominationMatchTag>(entity);
            AddComponent<DominationMatchSettings>(entity);
            
            AddComponent<DominationPlayersData>(entity);

        }
    }
}