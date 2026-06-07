using Unity.Entities;

public struct AssignCharacterToPlayer : IComponentData
{
    public Entity CharacterEntity;
    public Entity PlayerEntity;
}

public struct PlayerCharacterInitializedTag : IComponentData { }