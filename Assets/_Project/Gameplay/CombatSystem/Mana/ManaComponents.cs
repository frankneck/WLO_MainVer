using Unity.Entities;
using Unity.NetCode;

[GhostComponent]
public struct WeaponMaxMana : IComponentData
{ 
    [GhostField] public float Value;
}

[GhostComponent]
public struct CurrentMana : IComponentData
{
    [GhostField] public float Value;
}

public struct AccumulatedMana : IComponentData
{
    public float Value;
}

[GhostComponent]
public struct WeaponManaRecoveryRate : IComponentData
{
    [GhostField] public float Value; 
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct ManaSpendThisTickBuffer : ICommandData
{
    public NetworkTick Tick { get; set; }
    public float Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ManaSpendBuffer : IBufferElementData
{
    public float Value;
} 