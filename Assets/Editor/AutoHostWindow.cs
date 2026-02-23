#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AutoHostWindow : EditorWindow
{
    [MenuItem("Tools/Auto Host Settings")]
    public static void ShowWindow()
    {
        GetWindow<AutoHostWindow>("Auto Host Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Host в Editor", EditorStyles.boldLabel);

        bool current = BuildModeSettings.AutoHostInEditor;
        bool newValue = EditorGUILayout.Toggle("Enable AutoHost in Editor", current);
        if (newValue != current)
        {
            BuildModeSettings.AutoHostInEditor = newValue;
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Этот флаг управляет тем, будет ли в Editor автоматически запускаться нужная сцена/режим.", MessageType.Info);
    }
}
#endif