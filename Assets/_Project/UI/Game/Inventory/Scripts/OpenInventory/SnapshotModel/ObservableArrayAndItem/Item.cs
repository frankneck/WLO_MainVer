using System;

[Serializable]
public class Item
{
    public ItemId m_ItemId;
    public ItemDetails m_Details;
    public int m_Quantity;

    public Item(
        ItemId itemId, 
        ItemDetails itemDetails, 
        int quantity = 1)
    {
        m_ItemId = itemId;
        m_Details = itemDetails;
        m_Quantity = quantity;
    }
}


public class WeaponItem : Item
{
    public Container m_InnerContainer;
    public WeaponLevel m_Level;
    public int m_WeaponCapacity;
    public int m_CastSpellNumber;
    public float m_RecoveryRate;
    public float m_MaxMana;
    public float m_Spread;
    public bool m_Shuffle; 
    public float m_CastDelay; 
    
    public WeaponItem(    
        ItemId itemId, 
        ItemDetails itemDetails, 
        WeaponLevel level,
        int capacity,
        int quantity, 
        float manaRecoveryRate,
        float maxMana,
        float spreadDegrees,
        bool shuffle,
        float castDelay,
        int castSpellNumber)
        : base(itemId, itemDetails, quantity)
    {
        m_Level = level; 
        m_WeaponCapacity = capacity;
        m_RecoveryRate = manaRecoveryRate;
        m_MaxMana = maxMana;
        m_Spread = spreadDegrees;
        m_Shuffle = shuffle;
        m_CastDelay = castDelay;
        m_CastSpellNumber = castSpellNumber;
    }
}

public class SpellItem : Item
{
    public SpellItem(
        ItemId itemId, 
        ItemDetails itemDetails, 
        int quantity = 1) 
        : base(itemId, itemDetails, quantity)
    {
        
    }
}

public class ConsumableItem : Item
{
    public ConsumableItem(
        ItemId itemId, 
        ItemDetails itemDetails, 
        int quantity = 1) 
        : base(itemId, itemDetails, quantity)
    {
        
    }
}