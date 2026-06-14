using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Strip buffer data consist from weapon container data and consumable container data.
/// It used for selection equipped item and displaying. 
/// </summary>
[GhostComponent]
public struct CharacterEquipment : IBufferElementData
{
    [GhostField()] public Entity ItemEntity;
    [GhostField()] public int Quantity;
}

/// <summary>
/// Cashed for avoiding update every frame. Attach to character.
/// </summary>
public struct CharacterEquipmentCashedVersion : IComponentData
{
    public int CachedWeaponVersion;
    public int CachedConsumableVersion;
}