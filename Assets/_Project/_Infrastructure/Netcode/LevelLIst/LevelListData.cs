using Unity.NetCode;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Collections;
using System;

/// <summary>
/// Request to load level on level number (index) and bind to certain match entity 
/// </summary>
public struct LoadLevelAndBindToMatch : IComponentData
{
    public int LevelNumber;
    public Entity MatchEntity;
}

// Buffer 
public struct LevelListData : IBufferElementData
{
    public int LevelNumber;
    public EntitySceneReference Scene;
}