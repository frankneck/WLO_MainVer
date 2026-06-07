using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;

public class EquipmentWeaponWindow : VisualElement
{
    public Image Icon;
    public Slot[] Slots;
    public ItemId ItemId { get; private set; } = ItemId.Empty;

    private Label _capacityValueText;
    private Label _castSpellNumberValueText;
    private Label _castDelayValueText;
    private Label _shuffleValueText;
    private Label _spreadValueText;
    private Label _recoveryRateText;
    private Label _maxManaText;

    public EquipmentWeaponWindow(
        VisualTreeAsset template,
        int maxSlots)
    {
        template.CloneTree(this);
        this.AddToClassList("weapon-window__template");

        Icon = this.Q<Image>("itemIcon");

        var slotsContainer = this.Q<VisualElement>("slotsContainer");
        
        _shuffleValueText = this.Q<Label>("ShuffleValueText");
        _spreadValueText = this.Q<Label>("SpreadValueText");
        _capacityValueText = this.Q<Label>("CapacityValueText");
        _recoveryRateText = this.Q<Label>("RecoveryRateValueText");
        _maxManaText = this.Q<Label>("MaxManaValueText");
        _castDelayValueText = this.Q<Label>("CastDelayValueText");
        _castSpellNumberValueText = this.Q<Label>("CastSpellNumberValueText");
        
        slotsContainer.Clear();

        Slots = new Slot[maxSlots];

        for (int i = 0; i < Slots.Length; i++) 
        { 
            var slot = slotsContainer.CreateChild<Slot>("slot"); 
            slot.m_Type = SlotType.WeaponSlot; 
            Slots[i] = slot; 
        }
    }

    public void Set(EquipmentWindowViewData data)
    {
        if (data.Container == null)
        {
            Hide();
            return;
        }

        ItemId = data.ItemdId;
        
        Icon.image = data.Sprite != null ? data.Sprite.texture : null;
        style.display = DisplayStyle.Flex;
        
        string shuffleText = data.Shuffle ? "Yes" : "No"; 
        _shuffleValueText.text = $"Shuffle: {shuffleText}";

        _capacityValueText.text = $"Capacity: {data.Capacity} spells";
        
        _maxManaText.text = $"Max mana: {data.MaxMana} mana";
        
        _recoveryRateText.text = $"Recovery rate: {data.RecoveryRate} mana/sec";
        
        _spreadValueText.text = $"Spread: {data.Spread} degrees";

        _castDelayValueText.text = $"Cast delay: {data.CastDelay} sec";

        _castSpellNumberValueText.text = $"Cast number: {data.CastSpellNumber} spells";
        
        InitAndSetSlots(data.Container, data.Capacity);
    } 

    private void InitAndSetSlots(
        Container container, 
        int slotsNumber)
    {
        for (int i = 0; i < Slots.Length; i++)
        {        
            if (slotsNumber - 1 < i)
            {
                Slots[i].style.display = DisplayStyle.None;
                Slots[i].Set(new SlotViewData(), SlotState.Disactive);
                continue;
            }

            Slots[i].style.display = DisplayStyle.Flex;

            ItemId newId = ItemId.Empty;
            Sprite sprite = null;
            int qty = 1;
            
            var item = container.Items[i];

            if (item != null)
            {
                newId = item.m_ItemId;
                sprite = item.m_Details.Sprite;
                qty = item.m_Quantity;
            }

            var data = new SlotViewData
            {
                Owner = container.OwnerEntity,
                ItemId = newId,
                Sprite = sprite,
                Quantity = qty
            };

            Slots[i].Init(i, SlotType.WeaponSlot);
            Slots[i].Set(data, SlotState.Active);

            if (item == null)
                continue;
            
            // if item is weapon get weapon level (only weapon has level in cur.ver.)
            var level = item is WeaponItem weapon ? weapon.m_Level : WeaponLevel.None; 
            Slots[i].SetColor(level);
        }
    }

    public void Hide()
    {
        ItemId = ItemId.Empty;

        Icon.image = null;
        this.style.display = DisplayStyle.None;

        ClearSlots();
    }

    private void ClearSlots()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            // Empty
            var data = new SlotViewData();
            Slots[i].Set(data, SlotState.Disactive);
        }
    }
}