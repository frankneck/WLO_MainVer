using System;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class Slot : VisualElement {
    public Image m_Icon;
    public Label m_StackLabel;
    public VisualElement m_SlotFrame;
    public int m_Index { get; private set; }
    public ItemId m_StoredItemId;
    public Sprite m_BaseSprite;
    public SlotType m_Type;
    public SlotState m_State;
    public Entity m_EntityOwner;
    public int m_Quantity;
    
    // Stores all items 
    public Container m_Container; 

    public event Action<Vector2, Slot> OnStartDragEvent = delegate { };

    public VisualElement SlotFrame => m_SlotFrame; 

    private string m_CurrentLevelClass;

#region Public methods

    public Slot() {
        
        m_SlotFrame = this.CreateChild("slotFrame");
        
        m_SlotFrame.AddToClassList("slot-original__color");
        
        m_Icon = m_SlotFrame.CreateChild<Image>("slotIcon");
        m_StackLabel = m_SlotFrame.CreateChild<Label>("stackCount");

        RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    // Call to init Slot
    public void Init(int index, SlotType type)
    {
        m_Index = index;
        m_Type = type; 
    }

    // Call to update Slot
    public void Set(SlotViewData data, SlotState newState) 
    {
        // State of slot
        m_State = newState;

        m_EntityOwner = data.Owner; 
        m_StoredItemId = data.ItemId;

        m_BaseSprite = data.Sprite;
        m_Icon.image = m_BaseSprite != null ? m_BaseSprite.texture : null;
        
        m_Quantity = data.Quantity;
        m_StackLabel.text = m_Quantity > 1 ? m_Quantity.ToString() : string.Empty;
        m_StackLabel.visible = m_Quantity > 1;
    }

    /// <summary>
    /// Paint slot 
    /// </summary>
    public void SetColor(WeaponLevel level)
    {
        m_SlotFrame.RemoveFromClassList(m_CurrentLevelClass);

        m_CurrentLevelClass = level switch
        {
            WeaponLevel.Level1 => "level-one__color",
            WeaponLevel.Level2 => "level-two__color",
            WeaponLevel.Level3 => "level-three__color",
            WeaponLevel.Level4 => "level-four__color",
            WeaponLevel.Level5 => "level-five__color",
            _ => "level-none__color",
        };

        m_SlotFrame.AddToClassList(m_CurrentLevelClass);
    }

    public string GetCurrentLevelClass()
    {
        return m_CurrentLevelClass;
    }

    public void RemoveCurrentLevelClass()
    {
        m_SlotFrame.RemoveFromClassList(m_CurrentLevelClass);
    }

    public void RestoreCurrentLevelClass()
    {
        m_SlotFrame.AddToClassList(m_CurrentLevelClass);
    }


    public void Remove() 
    {   
        m_Type = SlotType.None;
        m_StoredItemId = ItemId.Empty;
        m_BaseSprite = null;
        m_Container = null;
        m_EntityOwner = Entity.Null;
    }

#endregion

#region Private methods

    private void OnPointerDown(PointerDownEvent evt) 
    {
        if (evt.button != 0 || m_StoredItemId.Equals(ItemId.Empty)) 
        {
            Debug.Log($"Button = {evt.button} and item id = {m_StoredItemId}");
            return;
        }

        OnStartDragEvent?.Invoke(evt.position, this);
        evt.StopPropagation();
    }

#endregion

}

/// <summary>
/// Important: Slot type must exactly appropriate Allowed slots. Else it doesn't work!
/// </summary>
[Flags]
public enum SlotType : byte
{
    None = 0,
    InventorySlot = 1 << 0,
    WeaponEquipmentSlot = 1 << 1,
    ConsumableEquipmentSlot = 1 << 2,
    WeaponSlot = 1 << 3
}

/// <summary>
/// All states that slot can have
/// </summary>
public enum SlotState : byte
{
    Disactive = 0,
    Active = 1,
}
