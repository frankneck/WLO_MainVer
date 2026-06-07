using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSpellSetConfig", menuName = "Weapon/New WeaponSpellSetConfig")]
public class WeaponSpellSetConfig : ScriptableObject
{
    [SerializeField] private AnimationCurve m_FillChangceCurve;
    [SerializeField] private SpellToSpawn[] m_Spells; 
    [SerializeField] private int m_MaxSlots; 

    public int MaxSlots => m_MaxSlots;

    public IReadOnlyList<SpellToSpawn> Spells => m_Spells;

    public float GetFillChance(int slot)
    {
        return m_FillChangceCurve.Evaluate(slot);
    }
}

[Serializable]
public struct SpellToSpawn
{
    public GameObject SpellPrefab;
    public int Weight;
}