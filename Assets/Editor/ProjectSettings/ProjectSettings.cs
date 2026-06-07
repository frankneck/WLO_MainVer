using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectSettings : EditorWindow
{
    private const string LevelListPath = "Assets/_Project/Data/Scenes/LevelList.asset";
    private const string DocumentPath = "Assets/Editor/UI/ProjectSettings.uxml";
    private const string AutoHostKey = "AutoHostKey";

    private LevelListSO _levelListSO;
    private VisualTreeAsset _documentTemplate;

    private VisualElement _levelsContainer;
    private Toggle _skipToggle;
    private Button _addButton;

    private const string MenuPath = "Tools/Project properties";
    private const string WindowName = "Project properties";

    [MenuItem(MenuPath)]
    public static void Open()
    {
        var window = GetWindow<ProjectSettings>();
        window.titleContent = new GUIContent(WindowName);
    }

    private void OnEnable()
    {
        _levelListSO = AssetDatabase.LoadAssetAtPath<LevelListSO>(LevelListPath);
        _documentTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(DocumentPath);
    }

    private void CreateGUI()
    {
        var root = rootVisualElement;
        _documentTemplate.CloneTree(root);

        _levelsContainer = root.Q<VisualElement>("LevelsContainer");
        _skipToggle = root.Q<Toggle>("skipMenuSettingToggle");
        _addButton = root.Q<Button>("AddLevelButton");

        _skipToggle.value = EditorPrefs.GetBool(AutoHostKey, false);

        _skipToggle.RegisterValueChangedCallback(evt =>
        {
            EditorPrefs.SetBool(AutoHostKey, evt.newValue);
        });

        _addButton.clicked += OnAddLevelClicked;

        Rebuild();
    }

    private void Rebuild()
    {
        _levelsContainer.Clear();

        var levels = _levelListSO.LevelList;

        for (int i = 0; i < levels.Count; i++)
        {
            int index = i;
            var scene = levels[i];

            var path = AssetDatabase.GetAssetPath(scene);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            var button = new Button(() =>
            {
                _levelListSO.SetSelectedIndex(index);
                EditorUtility.SetDirty(_levelListSO);
                Rebuild();
            })
            {
                text = name
            };

            if (index == _levelListSO.SelectedIndex)
                button.AddToClassList("active");

            _levelsContainer.Add(button);
        }
    }

    private void OnAddLevelClicked()
    {
        var filters = new[] { "Unity Scene", "unity" };

        string path = EditorUtility.OpenFilePanelWithFilters(
            "Select scene",
            Application.dataPath,
            filters);

        if (string.IsNullOrEmpty(path))
            return;

        path = "Assets" + path.Replace(Application.dataPath, "").Replace("\\", "/");

        if (LevelListEditorUtility.TryAdd(_levelListSO, path))
        {
            Rebuild();
        }
    }
}