using System;
using UnityEngine;

public static class CommandLineServerConfigService
{
    public static void SetServerConfig(out ushort port, out GameMode mode, out int sceneIndex)
    {
        port = 0;
        sceneIndex = 0;
        mode = GameMode.None;

        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--port")
            {
                if (!ushort.TryParse(args[i + 1], out port))
                {
                    Debug.LogError("Invalid server port");
                }
            }
            else if (args[i] == "--lvl")
            {
                if (!int.TryParse(args[i+1], out sceneIndex))
                {
                    if (!Application.CanStreamedLevelBeLoaded(sceneIndex))
                    {
                        Debug.LogError("Invalid level");
                    }
                }
            }
            else if (args[i] == "--mode")
            {
                if (args[i+1] == "Deathmatch")
                    mode = GameMode.Deathmatch;
                else if (args[i+1] == "Deathrace")
                    mode = GameMode.Domination;
                else
                {
                    UnityEngine.Debug.LogWarning("Attention! Current mode hasn't been assigned.");
                    return;
                }
            }
        } 
    }
}