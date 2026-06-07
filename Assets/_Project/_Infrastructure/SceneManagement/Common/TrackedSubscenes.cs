using Unity.Entities;

/// <summary>
/// Stores entity scene buffer 
/// </summary>
public struct TrackedSubscenes : IBufferElementData
{
    public Entity Entity;
}