using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Id for match
/// </summary>
public struct MatchIdComponent : IComponentData
{
    public MatchId Value;
}

public struct MatchTag : IComponentData { }

/// <summary>
/// Keeps nesseccery setting to start match
/// </summary>
public struct CreateMatchWithUserSettings : IComponentData
{
    public GameMode GameMode;
    public int LevelMap;
    public int MaxPlayers;
    
    public float DeathmatchRoundTime;
    public int DeathmatchNumberOfRounds;
    
    public int DominationMaxScore;
    public float DominationMatchTime;
    public float DominationRivavalTime;
}

/// <summary>
/// Marks that entity belnogs to match entity (entity -> match entity)
/// It used on the server side.
/// </summary>
[GhostComponent()]
public struct BelongsToMatch : IComponentData
{
   [GhostField()] public Entity Entity;
}

/// <summary>
/// It used on the client side.
/// </summary>
[GhostComponent()]
public struct BelongsToMatchId : IComponentData
{
    [GhostField()] public int MatchId;
}

public struct CreateTeam : IComponentData
{
    public TeamType TeamType;
    public MatchId MatchId;
    public Entity MatchEntity;
    public int NumberOfTeams;
}

/// <summary>
/// Starting state of match entity. Entity with this component starting new match
/// </summary>
public struct StartingMatchTag : IComponentData { }

/// <summary>
/// Playing state of match entity. Entity with this component is already playing game session
/// </summary>
public struct ActiveMatchTag : IComponentData { }

/// <summary>
/// Finishing state of match. Entity with this component is finishing game session
/// </summary>
public struct FinishingMatchTag : IComponentData { }

/// <summary>
/// Marks that the mathc doesn't have round entities and data
/// </summary>

public struct RoundCleanupInProgress : IComponentData { }

public struct DefineFinishMatchTick : IComponentData { }

public struct FinishMatchTickDefined : IComponentData { }

/// <summary>
/// Stores current round entity 
/// </summary>
public struct CurrentRoundEntityReference : IComponentData
{
    public Entity Entity;
}


/// <summary>
/// Defines what game mode on
/// </summary>
public enum GameMode : byte
{
    None = 0,
    Deathmatch,
    Domination
}

public struct MatchId
{
    private int m_Value;
    public MatchId(int value) => m_Value = value;

    public static MatchId Empty => new MatchId(-1); 

    public static implicit operator int(MatchId id) => id.m_Value; 
    public static explicit operator MatchId(int value) => new(value);
}