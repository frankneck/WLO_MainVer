using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class StorageView : MonoBehaviour 
{
    protected Slot[] m_Slots;

    protected UIDocument m_Document;

    protected static VisualElement m_GhostIcon;
    protected VisualElement m_Root;
    protected VisualElement m_Container;

    protected Slot m_CurrentDraggingSlot;
    protected SlotType m_CurrentDraggingType;

    private bool m_IsDragging;
    private string m_CashedLevelClass;

    public event Action<Slot, Slot> OnDropEvent;

    public void RegisterCallbacks()
    {
        m_Root.RegisterCallback<PointerUpEvent>(OnPointerUp);
        m_Root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        
        foreach (var slot in m_Slots)
        {
            slot.OnStartDragEvent += OnPoinerDown;        
        }
    }

    private void OnPoinerDown(Vector2 position, Slot slot)
    {
        m_CurrentDraggingType = slot.m_Type;
        m_CurrentDraggingSlot = slot;
        m_IsDragging = true;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // TODO: Place in other 
        if (PlayerInput.ShiftFastSlot.IsPressed())
        {
            // Change slot
            return;
        }

        if (slot.m_StoredItemId.Equals(ItemId.Empty)) 
            return;

        if (m_GhostIcon == null)
        {
            m_GhostIcon = new VisualElement();
            m_GhostIcon.style.position = Position.Absolute;
            m_GhostIcon.style.width = slot.m_Icon.resolvedStyle.width;
            m_GhostIcon.style.height = slot.m_Icon.resolvedStyle.height;
            m_Root.Add(m_GhostIcon);
        }

        m_CashedLevelClass = slot.GetCurrentLevelClass(); 
        m_GhostIcon.AddToClassList(m_CashedLevelClass);

        m_GhostIcon.style.backgroundImage = slot.m_BaseSprite.texture;
        m_GhostIcon.style.opacity = 1f;
        m_GhostIcon.style.visibility = Visibility.Visible;

        slot.m_Icon.image = null;
        slot.m_StackLabel.visible = false;
        slot.RemoveCurrentLevelClass();
    
        // position of mouse equal ghost icon position
        SetGhostIconPosition(position);
    }

    private void OnPointerUp(PointerUpEvent ect)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!m_IsDragging) return;
        
        Slot closestSlot = m_Slots
            .Where(slot => slot.worldBound.Overlaps(m_GhostIcon.worldBound) 
                && slot.m_State == SlotState.Active)
            .OrderBy(slot => Vector2.Distance(slot.worldBound.center, m_GhostIcon.worldBound.center))
            .FirstOrDefault();

        if (closestSlot == null)
        {
            if (m_CurrentDraggingSlot != null && m_CurrentDraggingSlot.m_BaseSprite != null)
            {
                m_CurrentDraggingSlot.RestoreCurrentLevelClass();
                m_CurrentDraggingSlot.m_Icon.image = m_CurrentDraggingSlot.m_BaseSprite.texture;
                m_CurrentDraggingSlot.m_StackLabel.visible = true;
            }
            
            m_GhostIcon.RemoveFromClassList(m_CashedLevelClass);
            m_GhostIcon.style.visibility = Visibility.Hidden;

            m_CurrentDraggingSlot = null;
            m_IsDragging = false;

            return;
        }

        m_GhostIcon.style.visibility = Visibility.Hidden;
        m_GhostIcon.RemoveFromClassList(m_CashedLevelClass);
        OnDropEvent?.Invoke(m_CurrentDraggingSlot, closestSlot);

#if UNITY_EDITOR
        Debug.Log($"Current closestSlot Index={closestSlot.m_Index}; Current closest slot type ={closestSlot.m_Type}. CurrentDraggingSlot Index={m_CurrentDraggingSlot.m_Index}. CurrentDraggingSlot type ={m_CurrentDraggingSlot.m_Type}.");
#endif

        m_CurrentDraggingSlot = null;
        m_IsDragging = false;

    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!m_IsDragging) return;

        SetGhostIconPosition(evt.position);
    }

    private void SetGhostIconPosition(Vector2 position)
    {
        m_GhostIcon.style.translate = new Translate(-25, -25, 0);
        m_GhostIcon.style.left = position.x;
        m_GhostIcon.style.top = position.y;
    }

    public abstract void Init(
        int inventoryCapacity, 
        int weaponEquipmentCapacity,
        int consumableEquipmentCapacity,
        int maxSlotsInWeapon, 
        ItemManagedDatabase itemManagedDatabase);

    public void Clear()
    {
        foreach (var slot in m_Slots)
            slot.Remove();
    }
}
