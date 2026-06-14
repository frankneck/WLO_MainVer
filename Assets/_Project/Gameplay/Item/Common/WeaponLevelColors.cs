using UnityEngine;

public class WeaponLevelColors
{
    private Color m_Default;
    private Color m_Level_1;
    private Color m_Level_2;
    private Color m_Level_3;
    private Color m_Level_4;
    private Color m_Level_5;

    public WeaponLevelColors(WeaponLevelColorConfig config)
    {
        m_Default = config.Default;
        m_Level_1 = config.Level_1;
        m_Level_2 = config.Level_2;
        m_Level_3 = config.Level_3;
        m_Level_4 = config.Level_4;
        m_Level_5 = config.Level_5;
    }

    public Color GetColor(WeaponLevel level) => level switch
    {
        WeaponLevel.Level1 => m_Level_1,
        WeaponLevel.Level2 => m_Level_2,
        WeaponLevel.Level3 => m_Level_3,
        WeaponLevel.Level4 => m_Level_4,
        WeaponLevel.Level5 => m_Level_5,
        _=> m_Default,
    };
}