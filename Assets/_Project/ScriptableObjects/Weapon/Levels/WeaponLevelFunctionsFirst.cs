using UnityEngine;

[CreateAssetMenu(fileName = "WeaponLevelConfig", menuName = "Weapon/New Weapon level config")]
public class WeaponLevelConfig : ScriptableObject
{
    [Header("Capacity")]
    [SerializeField] private int m_Capacity_k;
    [SerializeField] private int m_Capacity_b;

    [Header("Casting spells")]
    [SerializeField] private int m_CastingSpells_k;
    [SerializeField] private int m_CastingSpells_b;

    [Header("Max mana")]
    [SerializeField] private int m_MaxMana_k;
    [SerializeField] private int m_MaxMana_b;
    
    [Header("Float curves")]
    [SerializeField] private AnimationCurve m_CastDelay;
    [SerializeField] private AnimationCurve m_ManaRecoveryRate;
    [SerializeField] private AnimationCurve m_Spread;

    public float ApplyCastDelay(float x)
    {
        var value = Mathf.Round(m_CastDelay.Evaluate(x) * 100f) / 100f;
        return value;
    }

    public int ApplyCapacity(float x)
    {
        return (int)(m_Capacity_k * x + m_Capacity_b);
    }

    public int ApplyCastingSpells(float x)
    {
        return (int)(m_CastingSpells_k * x + m_CastingSpells_b);
    }

    public int ApplyMaxMana(float x)
    {
        return (int)(m_MaxMana_k * x + m_MaxMana_b);
    }

    public bool ApplyShuffle(float x)
    {
        return x == 1;
    }

    public float ApplySpread(float x)
    {
        var value = Mathf.Round(m_Spread.Evaluate(x) * 100f);
        return value;
    }

    public float ApplyManaRecoveryRate(float x)
    {
        var value = Mathf.Round(m_ManaRecoveryRate.Evaluate(x) * 100f);
        return value;
    }
}
