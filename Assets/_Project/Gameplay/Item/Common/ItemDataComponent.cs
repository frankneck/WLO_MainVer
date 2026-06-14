using System;
using Unity.Entities;
using UnityEngine;

public struct ItemTag : IComponentData { }

public struct WorldItemTag : IComponentData { }

/// <summary>
/// Stores data about Item (MaxStack, Name etc.)
/// </summary>
public struct CurrentItemId : IComponentData
{
    public ItemId Value;
}

/// <summary>
/// Stores immutable items data
/// </summary>
public struct ItemImmutableData
{
    public BlobArray<ItemData> ItemDataArray;
}

/// <summary>
/// Stores immutable Item Data 
/// </summary>
public struct ItemData
{
    public int MaxStack;
    public ItemType Type;
    public AllowedSlots AllowedSlots;
}

/// <summary>
/// Marks that item is collectable
/// </summary>
public struct CurrentPickupMode : IComponentData
{
    public PickupMode Mode;
}

public enum ItemType : byte
{
    Weapon,
    Spell,
    Consumable,
}

[Flags]
public enum PickupMode : byte
{
    None = 0,
    OnOverlap = 1 << 0,
    OnInteract = 1 << 1
}