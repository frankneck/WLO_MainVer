using System;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

[UnityEngine.Scripting.Preserve]
public class FrontendBootstrap : ClientServerBootstrap
{
    private const ushort DEFAULT_SERVER_PORT = 7979;

    public override bool Initialize(string defaultWorldName)
    {
        const string fallbackGameplayScene = "GameplayScene";
        const string frontendScene = "ClientConnectionScene";

        Debug.Log($"[FrontendBootstrap] startupTime: {Time.realtimeSinceStartupAsDouble:0.0}s, targetScene:!");

        if (!DetermineIfBootstrappingEnabled())
            return false;

        var activeScene = SceneManager.GetActiveScene().name;
        var targetScene = activeScene;

        var isFrontend = targetScene == frontendScene;

        if (IsServerPlatform)
        {
            if (isFrontend)
            {
                Debug.LogWarning($"[FrontendBootstrap] Server build loaded the isFrontend scene ('{activeScene}'), but cannot run it, so defaulting to {nameof(fallbackGameplayScene)}: '{fallbackGameplayScene}'!");
                targetScene = fallbackGameplayScene;
                isFrontend = false;
            }
        }
        else
            Debug.Log($"[FrontendBootstrap] This is running on the Client Platfrom. StartupTime: {Time.realtimeSinceStartupAsDouble:0.0}s, targetScene: '{targetScene}', isFrontend: {isFrontend}!");

#if UNITY_EDITOR
        Debug.Log($"[FrontendBootstrap] startupTime: {Time.realtimeSinceStartupAsDouble:0.0}s, targetScene: '{targetScene}', isFrontend: {isFrontend}!");
#endif
        
        AutoConnectPort = 0;

        if (isFrontend)
        {
            CreateLocalWorld(defaultWorldName);
        }
        else
        {
            ushort port = DEFAULT_SERVER_PORT;

            var args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--serverPort")
                {
                    if (!ushort.TryParse(args[i + 1], out port))
                    {
                        Debug.LogError("Invalid server port");
                    }
                }
            }
            
            WorldsManager.StartServer(port);
            // Scene dependency ...LoadSceneAsync(1, LoadSceneMode.Single);
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            asyncOperation.allowSceneActivation = true;
        }
        
        return true;
    }



    private static bool IsServerPlatform => Application.platform == RuntimePlatform.LinuxServer
                                            || Application.platform == RuntimePlatform.WindowsServer
                                            || Application.platform == RuntimePlatform.OSXServer;
}