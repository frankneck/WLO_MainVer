using Unity.Entities;

/// <summary>
/// Display active current level and state of loading scene
/// </summary>
public struct CurrentLevelSyncState : IComponentData
{
    public LevelSyncState State;
    public int CurrentLevel;
    public int NextLevel;
}

public enum LevelSyncState
{
    Idle,
    LevelLoadRequest,
    LevelLoadInProgress,
    LevelLoaded
}
