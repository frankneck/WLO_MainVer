using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WorldsManager
{
    private static WorldsMode _currentMode = WorldsMode.Local;
    public static WorldsMode currentMode
    {
        get => _currentMode; private set => _currentMode = value;
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

    public static void StartHost(ushort Port, string Address)
    {
        StartServer(Port);
        StartClient(Port, Address);
        currentMode = WorldsMode.Host;
    }

    public static void StartServer(ushort Port)
    {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Server World");

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }

        currentServerWorld = serverWorld;

        currentMode = WorldsMode.Server;
    }

    public static void StartClient(ushort Port, string Address)
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }

        World.DefaultGameObjectInjectionWorld = clientWorld;
        currentClientWorld = clientWorld;

        currentMode = WorldsMode.Client;
    }

    public static void StartLocalWorld(bool disposeNetWorlds = true)
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
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        loadSceneAsync.completed += (AsyncOperation _) =>
        {
            StartLocalWorld();
            LoadingScreenUI.Set(LoadingScreenState.None);
        };
    }

    public static void Disconnect(string message)
    {
        ClientConnectingStartTime = -1;
        LoadingScreenUI.Set(LoadingScreenState.Loading);
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        loadSceneAsync.completed += (AsyncOperation _) =>
        {
            StartLocalWorld();
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
