using Unity.Entities;
using Unity.NetCode;

public struct RoundTag : IComponentData { }

public struct RoundIdComponent : IComponentData
{
    public int Value;
}

/// <summary>
/// Marks that entity belnogs to round entity (entity -> round entity)
/// </summary>
public struct BelongsToRound : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// Counts for dethmatch mode. Adds in create round system
/// </summary>
public struct DeathmatchTeamAliveCounter : IComponentData  
{  
	public ushort RedAlive;  
	public ushort BlueAlive;  
}

public struct RoundPreInitializedTag : IComponentData, IEnableableComponent { }

public struct NewRoundTag : IComponentData, IEnableableComponent { }

public struct FixNumberOfPlayersInStartingRound : IComponentData, IEnableableComponent { }

/// <summary>
/// When players are ready start timer to start new round
/// </summary>
public struct StartingRoundTag : IComponentData, IEnableableComponent { }

/// <summary>
/// Activate round when first timer is ended
/// </summary>
public struct ActiveRoundTag : IComponentData, IEnableableComponent { }

/// <summary>
/// After timer or all players are eliminated. Need to define time to next timer
/// </summary>
public struct FinishingRoundTag : IComponentData, IEnableableComponent { }

public struct FinishedRoundTag : IComponentData, IEnableableComponent { }

/// <summary>
/// Stores number of players who is lived
/// </summary>
[GhostComponent()]
public struct AlivePlayers : IBufferElementData
{
    [GhostField()] public TeamType Team;
    [GhostField()] public int Value;
}

// Attach to round entity
public struct DefineStartRoundTick : IComponentData { }
public struct StartRoundTickDefined : IComponentData { }

public struct DefineEndRoundTick : IComponentData { }
public struct EndRoundTickDefined : IComponentData { }

public struct DefineFinishRoundTick : IComponentData { }
public struct FinishRoundTickDefined : IComponentData { }