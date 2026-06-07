using Unity.Entities;
using UnityEngine;

/// <summary>
/// For hud and inventory
/// </summary>
public struct SlotViewData
{
    public Entity Owner;
    public ItemId ItemId;
    public Sprite Sprite;
    public int Quantity;
    public WeaponLevel Level;
}