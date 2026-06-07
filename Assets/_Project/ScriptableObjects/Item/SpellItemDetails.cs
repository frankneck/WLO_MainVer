using UnityEngine;

[CreateAssetMenu(fileName = "SpellItemDetails", menuName = "Item/New SpellItemDetails")]
public class SpellItemDetails : ItemDetails
{
    [Header("Spell item parameters")]
    public GameObject ProjectilePrefab;
}