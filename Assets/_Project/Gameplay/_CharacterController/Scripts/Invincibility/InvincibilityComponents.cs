
using Unity.Entities;
using Unity.NetCode;

public struct InvincibilityTag : IComponentData { }

public struct InvincibilityTimer : IComponentData
{
    public float Value;
}

// Only Server
public struct InvincibilityEndAtTick : IComponentData
{
    public NetworkTick Tick;
}