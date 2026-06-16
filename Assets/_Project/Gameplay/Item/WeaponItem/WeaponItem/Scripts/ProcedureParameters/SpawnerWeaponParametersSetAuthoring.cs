using Unity.Entities;
using UnityEngine;

public class SpawnerWeaponParametersSetAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponParameterList _parametersConfig;
    
    class Baker : Baker<SpawnerWeaponParametersSetAuthoring>
    {
        public override void Bake(SpawnerWeaponParametersSetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddBuffer<SpawnerWeaponParamSet>(entity);
            
            var src = authoring._parametersConfig.Parameters;

            for (int i = 0; i < src.Count; i++)
            {
                AppendToBuffer(entity, new SpawnerWeaponParamSet
                {
                    Id = src[i].Id,
                    Type = src[i].Type,
                    Threshold = src[i].Threshold,
                    Step = src[i].Step,
                    MinValue = src[i].MinValue,
                    MaxValue = src[i].MaxValue
                });
            }
        }
    }
}