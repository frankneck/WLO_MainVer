using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponParameterList", menuName = "Scriptable Objects/WeaponParameterListConfig")]
public class WeaponParameterList : ScriptableObject
{
    [SerializeField] private WeaponParameter[] _parametersList;
    public IReadOnlyList<WeaponParameter> Parameters => _parametersList;
}
