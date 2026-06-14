using Unity.Entities;
using UnityEngine;

public class WeaponParametersSetAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponParameterList _parametersConfig;
    
    class Baker : Baker<WeaponParametersSetAuthoring>
    {
        public override void Bake(WeaponParametersSetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddBuffer<SpawnerParamSet>(entity);
            
            var src = authoring._parametersConfig.Parameters;

            for (int i = 0; i < src.Count; i++)
            {
                AppendToBuffer(entity, new SpawnerParamSet
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