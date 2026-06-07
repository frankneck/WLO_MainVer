using Unity.Entities;
using UnityEngine;

public class DeathmatchPlayerSpawnPointAuthoring : MonoBehaviour
{
    [SerializeField] private TeamType m_TeamType;
    [SerializeField] private Vector3[] m_Offsets;

    class Baker : Baker<DeathmatchPlayerSpawnPointAuthoring>
    {
        public override void Bake(DeathmatchPlayerSpawnPointAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<TeamSpawnPointTag>(entity);
            
            AddComponent(entity, new PlayerSpawnPointTeam
            {
                Value = authoring.m_TeamType
            });         

            var buffer = AddBuffer<PlayerSpawnPointOffset>(entity);
            
            foreach (var offset in authoring.m_Offsets)
            {
                buffer.Add(new PlayerSpawnPointOffset
                {
                    Value = offset
                });
            }
        }
    }
}