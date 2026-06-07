using Unity.Entities;
using Unity.NetCode;

[GhostComponent]
public struct WeaponSpellSet : IBufferElementData
{
    public Entity PrefabEntity;
    public int Weight;
}

public struct SlotFillChance : IBufferElementData
{
    public float Value;
} 