using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableItemDetails", menuName = "Item/New ConsumableItemDetails")]
public class ConsumableItemDetails : ItemDetails
{
    [Header("Consumable item parameters")]
    public ConsumableType ConsumableType;
}

public enum ConsumableType : byte
{
    Potion,
    Scroll
}