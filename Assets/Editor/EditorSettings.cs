#if UNITY_EDITOR
using UnityEditor;

public static class BuildModeSettings
{
    private const string PrefKey = "AutoHostInEditor";

    public static bool AutoHostInEditor
    {
        get => EditorPrefs.GetBool(PrefKey, true); // по умолчанию включено авто-хост в Editor
        set => EditorPrefs.SetBool(PrefKey, value);
    }
}
#endif