using UnityEngine;

/// <summary>
/// Inits gameplay data
/// </summary>
public class GameEntryPoint : MonoBehaviour
{
    [SerializeField] private WeaponLevelFunctionsSet m_set;
    private WeaponLevelFunctionsDatabase m_weaponDb;

    private void Start()
    {
        BindObjects();
        InitObjects();
    }

    private void BindObjects()
    {
        m_weaponDb = new WeaponLevelFunctionsDatabase();
    }

    private void InitObjects()
    {
        m_weaponDb.Init(m_set);
    }
}