using Unity.Entities;
using Unity.NetCode;

public struct ParamRaw : IBufferElementData
{
    public ParameterId Id;
    public float Value;
}

public struct ParamFinal : IBufferElementData
{
    public ParameterId Id;
    public float Value;
}

// Getting balanced values for weapon pipeline
public struct NeedRawGenerationTag : IEnableableComponent, IComponentData { }
public struct NeedNormalizationTag : IEnableableComponent, IComponentData { }
public struct NeedQuantizationTag : IEnableableComponent, IComponentData { }
public struct NeedCalculateBaseValueTag : IEnableableComponent, IComponentData { }
public struct NeedApplyFinalValuesTag : IEnableableComponent, IComponentData { }

[GhostComponent]
public struct CurrentWeaponLevel : IComponentData
{
    [GhostField] public WeaponLevel Value;
}

public struct WeaponMaxLevel : IComponentData
{
    public int Value;
}

public struct ParamDelta : IBufferElementData
{
    public float Value;
}

// Blob

public struct WeaponParametersReference : IComponentData
{
    public BlobAssetReference<WeaponParameters> Value;
}

public struct WeaponParameters
{
    public BlobArray<WeaponParameterBlob> Params;
}

public struct WeaponParameterBlob
{
    public ParameterId Id;

    public ParameterType Type;

    public BlobArray<float> BaseSamples;
    public BlobArray<float> LevelSamples;
    
    public float Threshold; // bool
    public float Step; // Int 
    
    public float Min; // 0
    public float Max;
}