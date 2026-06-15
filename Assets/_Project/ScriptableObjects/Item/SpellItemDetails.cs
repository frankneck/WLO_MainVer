using UnityEngine;

[CreateAssetMenu(fileName = "SpellItemDetails", menuName = "Item/New SpellItemDetails")]
public class SpellItemDetails : ItemDetails
{
    [Header("Spell item parameters")]
    public GameObject ProjectilePrefab;
    public SpellType SpellType;
    [Range(1, 1000)] public float ManaCost; 
    [Range(1, 1000)] public float Distance;
}