using Unity.Entities;

/// <summary>
/// Marks that all match item spawner should be activated before playing game
/// </summary>
public partial struct ActivateMatchItemSpawners : IComponentData
{
    public Entity MatchEntity;
}

/// <summary>
/// Marks that all match item spawner should be deactivated after playing game 
/// </summary>
public partial struct DeactivateMatchItemSpawners : IComponentData
{
    public Entity MatchEntity;
}