using System;
using Unity.Entities;

/// <summary>
/// Important: Allowed slots must exactly appropriate . Else it doesn't work!
/// </summary>
[Flags]
public enum AllowedSlots
{
    None = 0,
    InventorySlots = 1 << 0,
    WeaponEquipmentSlots = 1 << 1,
    ConsumableEquipmentSlots = 1 << 2,
    WeaponSlots = 1 << 3
}

// SlotType
