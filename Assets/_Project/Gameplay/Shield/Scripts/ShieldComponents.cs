using Unity.Entities;
using Unity.NetCode;

// DATA

public struct CharacterShieldState : IComponentData
{
    public bool IsActive;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct ShieldState : IComponentData
{
    [GhostField] public bool IsActive;
}

public struct ShieldActivated : IComponentData { } 


public struct PredictedShieldTag : IComponentData { }

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct ShieldEntityReference : IComponentData
{
    [GhostField] public Entity Entity;
}

// VISUAL RENDER

public struct VisualRenderShieldCreated : IComponentData { }

public struct VisualRenderShieldTag : IComponentData { } 

public struct VisualRenderShieldActivated : IComponentData { }

public struct VisualRenderShieldEntityReference : IComponentData
{
    public Entity Entity;
}