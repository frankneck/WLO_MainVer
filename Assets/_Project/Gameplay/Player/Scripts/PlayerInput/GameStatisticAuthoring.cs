using Unity.Entities;
using UnityEngine;

public class GameStatisticAuhtoring : MonoBehaviour
{
    class GameStatisticBaker : Baker<GameStatisticAuhtoring>
    {
        public override void Bake(GameStatisticAuhtoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<KDCounter>(entity);
        }
    }
}