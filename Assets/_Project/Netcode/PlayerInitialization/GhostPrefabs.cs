using System;
using Unity.Entities;

[Serializable]
public struct GhostPrefabs : IComponentData
{
    public Entity CharacterPrefab;
    public Entity PlayerPrefab;
}