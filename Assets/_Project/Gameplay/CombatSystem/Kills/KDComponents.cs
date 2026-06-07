using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;

public struct LastDamager : IComponentData
{
    public Entity Entity;
}

public struct UpdateKDRequest : IComponentData
{
    public KillEvent Value;
}

public enum KillEvent : byte
{
    Kill = 1,
    Death = 0
}

public struct KillRequest : IComponentData
{
    public Entity Killer;
    public Entity Victim;
    public Entity MatchEntity;
}