
using System;

/// <summary>
/// This has Item ID Value. If you want convert from int to ItemID, you need explicit conversion.
/// <para> For example ItemID id = (ItemID)10</para>   
/// </summary>
[Serializable]
public struct ItemId
{
    public int Value;
    public static ItemId Empty = new ItemId(-1);

    public ItemId(int value) => Value = value;
    
    public static implicit operator int(ItemId id) => id.Value;
    public static explicit operator ItemId(int value) => new(value); 
}