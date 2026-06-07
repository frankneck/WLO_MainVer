using Unity.NetCode;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Collections;
using System;

// Common
public struct LoadLevelRequest : IComponentData
{
    public int LevelNumber;
}

// Buffer 
public struct LevelListData : IBufferElementData
{
    public int LevelNumber;
    public EntitySceneReference Scene;
}