using Unity.Entities;

/// <summary>
/// Stores prefabs of game mode
/// </summary>
public struct GameModesPrefabs : IComponentData
{
    public Entity Deathrace;
    public Entity Deathmatch;
    public Entity RoundEntityPrefab;
}