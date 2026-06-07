using Unity.Entities;

/// <summary>
/// Marks that health potion
/// </summary>
public partial struct HealthPotionTag : IComponentData { }

/// <summary>
/// Stores value that health potion recovers
/// </summary>
public partial struct HealthPotionVolume : IComponentData
{
    public float Value;
}