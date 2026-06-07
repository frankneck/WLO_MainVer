using Unity.Entities;

public struct WeaponParamSet : IBufferElementData
{
    public ParameterId Id;
    public ParameterType Type;
    public float Threshold;
    public float Step;
    public float MinValue;
    public float MaxValue;
}