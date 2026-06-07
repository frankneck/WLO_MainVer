using System;
using System.Collections.Generic;
using Unity.Entities;

public class Container : IContainer
{
    public string Name { get; } 

    // Container has container Entity. 
    // It needs to send to server from and where we place item
    public Entity OwnerEntity { get; }
    
    public ObservableArray<Item> Items { get; set; }

    public event Action<Item[]> OnModelContainerChanged
    {
        add => Items.AnyValueChanged += value;
        remove => Items.AnyValueChanged -= value;
    }

    public Container(
        Entity containerEntityOwner, 
        string name, int capacity = 20, 
        IEnumerable<ItemDetails> details = null)
    {
        Items = new ObservableArray<Item>(capacity);
        Name = name;
        OwnerEntity = containerEntityOwner;
    }

    public Item Get(int index) => Items[index];

    public bool TryGetItem(int index, out Item item)
    {
        item = null;

        if (Items == null)
            return false;

        if (index < 0 || index >= Items.Capacity)
            return false;

        item = Items[index];
        return item != null;
    }

    public void Set(Item item, int index) => Items.Set(item, index);

    public bool CanPlace(Item item, Slot slot) => (item.m_Details.AllowedSlots & (AllowedSlots)slot.m_Type) != 0;
}