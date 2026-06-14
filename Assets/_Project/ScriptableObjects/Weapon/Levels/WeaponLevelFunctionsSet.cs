using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponLevelFunctionsSet", menuName = "Scriptable Objects/WeaponLevelFunctionsSet")]
public class WeaponLevelFunctionsSet : ScriptableObject
{
    [SerializeField] private List<WeaponLevelConfig> m_WeaponlevelConfigs;
    public IReadOnlyList<WeaponLevelConfig> LevelFunctions => m_WeaponlevelConfigs; 
}