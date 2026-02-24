using Unity.Entities;
using UnityEngine;

public class TeamAuthoring : MonoBehaviour
{
    public TeamType Team;

    class TeamBaker : Baker<TeamAuthoring>
    {
        public override void Bake(TeamAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new GameTeam { Value = authoring.Team });
        }
    }
}
