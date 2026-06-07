using System;
using UnityEngine;

/// <summary>
/// Base class for item details
/// </summary>
public abstract class ItemDetails : ScriptableObject
{
    [Header("Common parameters")]
    [SerializeField] public ItemId Id; 
    
    public Sprite Sprite;
    
    public string Name;
    public string Description;
    
    [Range(1, 64)] public int MaxStack;
    
    public AllowedSlots AllowedSlots;
    public ItemType Type;
}