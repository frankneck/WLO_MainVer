using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

[UnityEngine.Scripting.Preserve]
public class CustomClientServerBootstrap : ClientServerBootstrap
{
    const ushort DEFAULT_SERVER_PORT = 7979;

    public override bool Initialize(string defaultWorldName)
    {
        Debug.Log($"[CustomClientServerBootstap] StartupTime: {Time.realtimeSinceStartupAsDouble:0.0}s");

        if (!DetermineIfBootstrappingEnabled())
            return false;
        
        AutoConnectPort = 0;

#if UNITY_SERVER
        StartDedicatedServer();
#else
        StartClient();
#endif

        return true;
    }

    private void StartDedicatedServer()
    {
        ushort port = DEFAULT_SERVER_PORT;

        CommandLineServerConfigService.SetServerConfig(out port, out GameMode mode, out int levelMap);
        
        WorldsManager.CreateServerWorld(
            port: port, 
            gameMode: mode, 
            levelMap: levelMap, 
            maxPlayers: 12,  
            
            deathmatchRoundTime: 60, 
            deathmatchNumberOfRounds: 5, 
            
            dominationMatchTime: 10,
            dominationMaxScore: 100,
            dominationRivaleTime: 5
        );

        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);     
        
        Debug.Log($"[CustomClientServerBootstap] Server started with port {port} and loaded level {levelMap}");
    }

    private void StartClient()
    {
        CreateLocalWorld("Local World");
    }
}

