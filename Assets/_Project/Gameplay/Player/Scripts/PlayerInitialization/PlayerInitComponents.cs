using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct ClientPlayerInitRequest : IComponentData
{
    public GameMode GameMode;
    public FixedString128Bytes Nickname;
    public TeamType Team;
} 

public struct PlayerInitRequest : IRpcCommand
{
    public GameMode GameMode;
    public FixedString128Bytes PlayerName;
    public TeamType TeamValue;
}

public struct ServerPlayerInitRequest : IComponentData
{
    public FixedString128Bytes PlayerName;
    public GameMode GameMode;
    public TeamType TeamValue;
}

public struct GameTeam : IComponentData
{
    [GhostField] public TeamType Value;
}

public struct PlayerPing : IComponentData
{
    [GhostField] public ushort Value;
}


public struct PlayerName : IComponentData
{
    [GhostField] public FixedString128Bytes Value;
}

public struct CashedCharacterData : IComponentData
{
    public FixedString128Bytes CharacterName;
}

public struct NewCharacterPlayerTag : IComponentData { }

public struct ReadyPlayerCharacterSpawn : IComponentData { }

public struct LocalInitialized : IComponentData { }

public enum TeamType : byte
{
    None = 0,
    Spectator = 1,
    Red = 2,
    Blue = 3,
}