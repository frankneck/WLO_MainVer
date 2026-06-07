using Unity.Entities;
using UnityEngine;

/// <summary>
/// Stores all view prefabs of player character
/// </summary>
public partial struct PlayerCharacterViews : IComponentData
{
    public Entity TPView;
}

/// <summary>
/// Request to create view entity for player character entity
/// </summary>
public partial struct CreatePlayerCharacterViewRequest : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// Cashed player character view 
/// </summary>
public partial struct LastPlayerCharacterView : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// View has owner player character (view entity -> owner character entity)
/// </summary>
public partial struct PlayerCharacterViewOwner : IComponentData
{
    public Entity Entity;
}

public partial struct ThirdPersonPlayerCharacterTag : IComponentData { }
public partial struct HasPlayerCharacterViewTag : IComponentData { }
