using System;
using Unity.Entities;

[Serializable]
public struct GhostPrefabs : IComponentData
{
    // Player logic

    public Entity CharacterPrefab;
    public Entity PlayerPrefab;
    
    // Game logic
    
    public Entity RespawnEntity;

    // NPC 

    public Entity NpcEntity;

    // Item Container
    
    public Entity ItemContainer;
}