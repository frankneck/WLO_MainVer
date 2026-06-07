#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelList", menuName = "Configs/Level List")]
public class LevelListSO : ScriptableObject
{
    [SerializeField] private List<SceneAsset> _levelList = new();
    [SerializeField] private int _selectedIndex = -1;

    public IReadOnlyList<SceneAsset> LevelList => _levelList;
    public int SelectedIndex => _selectedIndex;

    public void SetSelectedIndex(int index)
    {
        if (index < 0 || index >= _levelList.Count)
            return;

        _selectedIndex = index;
    }
}
#endif