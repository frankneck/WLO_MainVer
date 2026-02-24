using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct ClientPlayerInitRequest : IComponentData
{
    public FixedString128Bytes Nickname;
    public TeamRequest Team; 
} 

public struct PlayerInitRequest : IRpcCommand
{
    public FixedString128Bytes PlayerName;
    public TeamRequest TeamValue;
}

public struct ServerPlayerInitRequest : IComponentData
{
    public FixedString128Bytes PlayerName;
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

public struct NewPlayerTag : IComponentData { }

public struct ReadySpawn : IComponentData { }

public struct LocalInitialized : IComponentData { }

public enum TeamType : byte
{
    Blue = 1,
    Red = 2,
    Spectator = 0
}