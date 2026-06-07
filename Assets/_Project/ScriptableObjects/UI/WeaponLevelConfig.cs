using UnityEngine;

[CreateAssetMenu(fileName = "WeaponLevelColorConfig", menuName = "UI/New weapon item level color config")]
public class WeaponLevelColorConfig : ScriptableObject
{
    public Color Default;
    public Color Level_1;
    public Color Level_2;
    public Color Level_3;
    public Color Level_4;
    public Color Level_5;
}