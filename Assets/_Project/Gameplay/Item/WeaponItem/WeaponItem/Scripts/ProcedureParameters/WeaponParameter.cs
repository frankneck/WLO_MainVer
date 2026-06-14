using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct WeaponParameter 
{
    public ParameterId Id;
    public ParameterType Type;
    [Range(0, 1)] public float Threshold; 
    [Range(0, 1)] public float Step; 
    [Range(0, 1)] public float MinValue;
    [Range(0, 1)] public float MaxValue;
}

public enum ParameterId : byte
{
    Shuffle = 0,
    CastingSpells = 1,
    CastDelay = 2,
    MaxMana = 3,
    RegenerationManaSpeed = 4,
    Capacity = 5,
    Spread = 6
}

public enum ParameterType : byte
{
    Float = 0,
    Int = 1,
    Bool = 2
}
