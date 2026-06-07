using Unity.Entities;
using Unity.NetCode;

public struct DestroyVisualRenderEntityTag : IComponentData { }

public struct DestroyEntityTag : IComponentData { }

public struct DestroyOnTimer : IComponentData
{
    public float Value;
}

public struct DestroyAtTick : IComponentData
{
    [GhostField] public NetworkTick Value;
}