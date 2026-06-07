using Unity.Entities;

/// <summary>
/// This component stores visual prefabs for other objects (e.g. shield, weapon etc.)
/// </summary>
public struct VisualPrefabs : IComponentData
{
    public Entity Shield;
}