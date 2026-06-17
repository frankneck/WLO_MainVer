using UnityEngine;
using Unity.Entities;

public class SpawnerWeaponLevelAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponLevel m_WeaponLevel;
    
    class Baker : Baker<SpawnerWeaponLevelAuthoring>
    {
        public override void Bake(SpawnerWeaponLevelAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // level
            AddComponent(entity, new SpawnerWeaponLevel 
            { 
                Value = authoring.m_WeaponLevel 
            });
        }
    }
}