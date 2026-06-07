#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Has methods to work with level list in Editor
/// </summary>
public static class LevelListEditorUtility
{
    /// <summary>
    /// Try to add new level into level list 
    /// </summary>
    public static bool TryAdd(LevelListSO list, string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        if (scene == null)
            return false;

        var so = new SerializedObject(list);
        var prop = so.FindProperty("_levelList");

        for (int i = 0; i < prop.arraySize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == scene)
                return false;
        }

        prop.arraySize++;
        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = scene;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(list);

        return true;
    }
}
#endif