using Unity.Entities;

// Taget for visual render entity. If entity has this component with Null Entity value CleanupVisualRenderEntitySystem are called
public struct VisualRenderGhostTarget : IComponentData
{
    public Entity Entity;
}

// Need to define character's owner for visual render entity on the client
public struct VisualRenderCharacterTarget : IComponentData
{
    public Entity Entity;
}
