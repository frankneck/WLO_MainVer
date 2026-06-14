using Unity.Entities;
using UnityEngine;

class WeaponParametersAuthoring : MonoBehaviour
{
    class WeaponParametersAuthoringBaker : Baker<WeaponParametersAuthoring>
    {
        public override void Bake(WeaponParametersAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddBuffer<WeaponParamSet>(entity);
            AddBuffer<ParamRaw>(entity);
            AddBuffer<ParamDelta>(entity);
            AddBuffer<ParamFinal>(entity);

            AddComponent<NeedRawGenerationTag>(entity);
            SetComponentEnabled<NeedRawGenerationTag>(entity, false);
            
            // Default: false
            AddComponent<NeedNormalizationTag>(entity);
            SetComponentEnabled<NeedNormalizationTag>(entity, false);

            AddComponent<NeedQuantizationTag>(entity);
            SetComponentEnabled<NeedQuantizationTag>(entity, false);

            AddComponent<NeedCalculateBaseValueTag>(entity);
            SetComponentEnabled<NeedCalculateBaseValueTag>(entity, false);
            
            AddComponent<NeedApplyFinalValuesTag>(entity);
            SetComponentEnabled<NeedApplyFinalValuesTag>(entity, false);
            
            // 1 level is default
            AddComponent(entity, new CurrentWeaponLevel { Value = WeaponLevel.Level1 });
        }
    }
}

