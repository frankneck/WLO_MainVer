using Unity.Entities;
using Unity.NetCode;

public partial struct MaxHitPoints : IComponentData
{
    public int Value;
}

[GhostComponent]
public partial struct CurrentHitPoints : IComponentData
{
    [GhostField] 
    public int Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public partial struct DamageBufferElement : IBufferElementData
{
    public int Value;
}

// From server to non owning client 
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public partial struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int Value;
}