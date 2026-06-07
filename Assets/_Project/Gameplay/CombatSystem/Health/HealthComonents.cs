using Unity.Entities;
using Unity.NetCode;

public struct MaxHealth : IComponentData
{
    public float Value;
}

[GhostComponent]
public struct CurrentHealth : IComponentData
{
    [GhostField] 
    public float Value;
}

public struct HealthRegenerationSpeed : IComponentData
{
    public float Value;
} 

public struct RegenerationHealthAccumulated : IComponentData
{
    public float Value;
}