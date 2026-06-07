using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class DualBuildToolPro : EditorWindow
{
    TextField buildPath;
    Toggle devToggle;
    Toggle serverToggle;
    EnumField versionField;

    VisualElement scenesContainer;
    bool[] sceneSelection;


    [MenuItem("Tools/Dual Build Tool Pro")]
    public static void Open() => GetWindow<DualBuildToolPro>().titleContent = new GUIContent("Dual Build Pro");

    void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.paddingLeft = 10;
        root.style.paddingTop = 10;

        root.Add(new Label("Build Folder"));
        buildPath = new TextField { value = "Builds" };
        root.Add(buildPath);

        root.Add(new Button(() =>
        {
            string path = EditorUtility.OpenFolderPanel("Build folder", "", "");
            
            if (!string.IsNullOrEmpty(path)) 
                buildPath.value = path;

        }) { text = "Browse" });

        devToggle = new Toggle("Development Build");
        root.Add(devToggle);

        serverToggle = new Toggle("Build Server") { value = true };
        root.Add(serverToggle);

        versionField = new EnumField("Build Status", AppVersionStatus.INDEV);
        root.Add(versionField);

        root.Add(new Label("Scenes"));
        scenesContainer = new VisualElement();
        root.Add(scenesContainer);
        LoadScenes();

        var buildBtn = new Button(StartBuild) { text = "BUILD" };
        buildBtn.style.marginTop = 10;
        root.Add(buildBtn);
    }

    void LoadScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        
        sceneSelection = new bool[scenes.Length];
        scenesContainer.Clear();

        for (int i = 0; i < scenes.Length; i++)
        {
            int index = i;
            var toggle = new Toggle(Path.GetFileNameWithoutExtension(scenes[i].path)) { value = scenes[i].enabled };
            
            sceneSelection[i] = toggle.value;
            toggle.RegisterValueChangedCallback(evt => sceneSelection[index] = evt.newValue);
            scenesContainer.Add(toggle);
        }
    }

    void StartBuild()
    {
        try
        {
            string root = Path.GetFullPath(buildPath.value);
            string version = GetVersionString();
            
            string buildRoot = Path.Combine(root, version);

            PlayerSettings.bundleVersion = version;
            Debug.Log($"[DualBuildTool] Set bundleVersion={version}");

            // Creating directories
            Directory.CreateDirectory(buildRoot);
            
            string clientDir = Path.Combine(buildRoot, "Client");
            string serverDir = Path.Combine(buildRoot, "Server");

            Directory.CreateDirectory(clientDir);
            Directory.CreateDirectory(serverDir);

            string[] scenes = GetSelectedScenes();
            
            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes selected!");
                return;
            }

            if (!BuildClient(clientDir, scenes)) return;
            if (serverToggle.value && !BuildServer(serverDir, scenes)) return;

            ZipBuild(buildRoot);

            EditorUtility.ClearProgressBar();
            UnityEngine.Debug.Log("BUILD SUCCESS");
            Process.Start("explorer.exe", buildRoot);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"BUILD FAILED: {e}");
        }
    }

    string[] GetSelectedScenes() => EditorBuildSettings.scenes.Where((s, i) => sceneSelection[i]).Select(s => s.path).ToArray();

    bool BuildClient(string path, string[] scenes)
    {
        EditorUtility.DisplayProgressBar("Build", "Building Client", 0.3f);
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(path, "Game.exe"),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = devToggle.value ? BuildOptions.Development : BuildOptions.None
        };
        
        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result != BuildResult.Succeeded) 
        { 
            UnityEngine.Debug.LogError("Client build failed"); 
            
            return false; 
        }

        return true;
    }

    bool BuildServer(string path, string[] scenes)
    {
        EditorUtility.DisplayProgressBar("Build", "Building Server", 0.6f);
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(path, "Server.exe"),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };
        
        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result != BuildResult.Succeeded) 
        { 
            UnityEngine.Debug.LogError("Server build failed"); 
            
            return false; 
        }
        
        return true;
    }

    void ZipBuild(string buildRoot)
    {
        EditorUtility.DisplayProgressBar("Build", "Creating ZIP", 0.9f);
        string zip = buildRoot + ".zip";
        
        if (File.Exists(zip)) 
            File.Delete(zip);
        
        try 
        { 
            ZipFile.CreateFromDirectory(buildRoot, zip); 
        }
        catch (Exception e) 
        { 
            UnityEngine.Debug.LogError($"ZIP failed: {e}"); 
        }
    }

    string GetVersionString()
    {
        var status = (AppVersionStatus)versionField.value;
        return $"{status} {FormatVersionFromUtc(DateTime.UtcNow)}";
    }

    string FormatVersionFromUtc(DateTime utc)
    {
        return string.Format("{0:D2}{1:D2}{2:D2}{3:D2}{4:D2}",
            utc.Year % 100,
            utc.Day,
            utc.Month,
            utc.Hour,
            utc.Minute);
    }
}

public enum AppVersionStatus : byte
{
    INDEV,
    PLAYTEST,
    ALPHA,
    BETA
}