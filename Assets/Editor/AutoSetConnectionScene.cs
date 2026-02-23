using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoSetConnectionScene
{
    private const string scenePath = "Assets/_Project/Scenes/ClientConnectionScene/ClientConnectionScene.unity";
    private const string sessionKey = "AutoSetConnectionScene.previousScenePath";

    static AutoSetConnectionScene()
    {
        // Подписываемся на событие запуска в режиме Play
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Когда редактор собирается перейти в PlayMode (ExitingEditMode), переключаем сцену
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Если нужная сцена не открыта, открываем её
            var activeScene = EditorSceneManager.GetActiveScene();
            SessionState.SetString(sessionKey, activeScene.path);
            if (activeScene.path != scenePath)
            {
                // Сохраняем изменения в текущей сцене, если нужно
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(scenePath);
                else
                    EditorApplication.isPlaying = false;
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            var prev = SessionState.GetString(sessionKey, string.Empty);
            if (!string.IsNullOrEmpty(prev))
            {
                EditorSceneManager.OpenScene(prev);
                SessionState.EraseString(sessionKey);
            }
        }
    }
}
