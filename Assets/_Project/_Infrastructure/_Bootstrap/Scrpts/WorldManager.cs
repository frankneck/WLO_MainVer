using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// API that stores world creating methods
/// </summary>
public static class WorldsManager
{
    private static WorldsMode _currentMode = WorldsMode.Local;
    public static WorldsMode currentMode
    {
        get => _currentMode; 
        private set => _currentMode = value;
    }

    public static World currentLocalWorld;
    public static World currentServerWorld;
    public static World currentClientWorld;

    public static float ClientConnectTimeout = 20f;
    public static float ClientConnectingStartTime = -1f;

    public static void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
        currentLocalWorld = null;
    }

    public static void StartHost(
        ushort port, 
        string address, 
        GameMode gameMode,
        int levelMap,
        int maxPlayers,
        
        int deathmatchRoundTime,
        int deathmatchNumberOfRounds,
        
        int dominationMaxScore,
        int dominationMatchTime,
        int dominationRivaleTime
    )
    {
        CreateServerWorld(
            port: port, 
            gameMode: gameMode, 
            maxPlayers: maxPlayers, 
            levelMap: levelMap,
            
            deathmatchRoundTime: deathmatchRoundTime, 
            deathmatchNumberOfRounds: deathmatchNumberOfRounds,
            
            dominationMaxScore: dominationMaxScore,
            dominationMatchTime: dominationMatchTime,
            dominationRivaleTime: dominationRivaleTime
        );

        CreateClientWorld(
            port, 
            address
        );

        currentMode = WorldsMode.Host;
    }

    /// <summary>
    /// Creates server world and listens port, optionally loads start level on the server
    /// </summary>
    public static void CreateServerWorld(
        ushort port, 
        GameMode gameMode,
        int maxPlayers,
        int levelMap,
        
        int deathmatchNumberOfRounds,
        int deathmatchRoundTime,
        
        int dominationMaxScore,
        int dominationMatchTime,
        int dominationRivaleTime
    )
    {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Server World");
        EntityManager em = serverWorld.EntityManager;

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }

        if (levelMap < 0)
        {
            Debug.LogError("Invalid number of level map.");
            return;
        }

        currentServerWorld = serverWorld;
        currentMode = WorldsMode.Server;

        UnityEngine.Debug.Log($"Create new match with next settings: {deathmatchRoundTime}; {maxPlayers}; {deathmatchNumberOfRounds}; {levelMap}; {gameMode}.");

        var createNewMatchRequest = em.CreateEntity();
        em.AddComponentData(createNewMatchRequest, new CreateMatchWithUserSettings  
        { 
            LevelMap = levelMap,
            GameMode = gameMode,
            MaxPlayers = maxPlayers,
            
            DeathmatchNumberOfRounds = deathmatchNumberOfRounds,
            DeathmatchRoundTime = deathmatchRoundTime,
            
            DominationMaxScore = dominationMaxScore,
            DominationMatchTime = dominationMatchTime,
            DominationRivavalTime = dominationRivaleTime,
        });
    }

    public static void CreateClientWorld(ushort Port, string Address)
    {
        World clientWorld = ClientServerBootstrap.CreateClientWorld("Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }

        World.DefaultGameObjectInjectionWorld = clientWorld;
        currentClientWorld = clientWorld;

        currentMode = WorldsMode.Client;
    }

    public static void CreateLocalWorld(bool disposeNetWorlds = true)
    {
        var toDispose = new List<World>();
        foreach (var w in World.All)
        {
            if ((w.Flags & (WorldFlags.GameClient | WorldFlags.GameServer | WorldFlags.GameThinClient)) != 0)
                toDispose.Add(w);
        }
        foreach (var w in toDispose)
            w.Dispose();

        World localWorld = ClientServerBootstrap.CreateLocalWorld("Local World");
        World.DefaultGameObjectInjectionWorld = localWorld;

        currentClientWorld = null;
        currentServerWorld = null;

        currentMode = WorldsMode.Local;
    }

    public static void Disconnect()
    {
        ClientConnectingStartTime = -1;
        LoadingScreenUI.Set(LoadingScreenState.Loading);
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        loadSceneAsync.completed += (AsyncOperation _) =>
        {
            CreateLocalWorld();
            LoadingScreenUI.Set(LoadingScreenState.None);
        };
    }

    public static void Disconnect(string message)
    {
        ClientConnectingStartTime = -1;
        LoadingScreenUI.Set(LoadingScreenState.Loading);
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        loadSceneAsync.completed += (AsyncOperation _) =>
        {
            CreateLocalWorld();
            LoadingScreenUI.Set(LoadingScreenState.None);
            ModalUI.OpenModal("Disconnect", message);
        };
    }
}

public enum WorldsMode : byte
{
    Host,
    Server,
    Client,
    Local
}
