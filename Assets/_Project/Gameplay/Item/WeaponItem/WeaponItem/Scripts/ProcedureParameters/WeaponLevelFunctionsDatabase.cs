using UnityEngine;

public class WeaponLevelFunctionsDatabase
{
    public static WeaponLevelFunctionsDatabase Instance  { get; private set; }
    public WeaponLevelFunctionsSet Set { get; private set; }

    public void Init(WeaponLevelFunctionsSet set)
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Database already initialized");
            return;
        }

        Instance = this;
        Set = set;
    }
}