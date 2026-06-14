using UnityEngine;
using Unity.Entities;
using UnityEditor;
using Unity.NetCode;

public class WeaponLevelAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponLevel m_WeaponLevel;
    
    class Baker : Baker<WeaponLevelAuthoring>
    {
        public override void Bake(WeaponLevelAuthoring authoring)
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