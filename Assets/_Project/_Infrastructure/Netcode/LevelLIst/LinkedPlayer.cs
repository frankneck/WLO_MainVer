using Unity.Entities;

// This component is linked to connection entity to know player
public struct LinkedPlayerCharacter : IComponentData
{
    public Entity Player;
}