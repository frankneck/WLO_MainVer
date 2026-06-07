using Unity.Entities;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public partial struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public partial struct DamageBufferElement : IBufferElementData
{
    public int Value;
}

public struct DamageOnTrigger : IComponentData
{
    public int Value;
}

public struct AlreadyDamagedEntity : IBufferElementData
{
    public Entity Value;
}

// This stat of game for player entity (on player entity)
[GhostComponent]
public struct KDCounter : IComponentData
{
    [GhostField] public int Kills;
    [GhostField] public int Deaths;
}

public struct DamageMultiplier : IComponentData
{
    public float Multiplier;
}