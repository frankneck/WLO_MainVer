using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ClientConnectionEventListener : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamDriver>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if(WorldsManager.ClientConnectingStartTime != -1)
        {
            if(SystemAPI.Time.ElapsedTime > WorldsManager.ClientConnectingStartTime + WorldsManager.ClientConnectTimeout)
            {
                WorldsManager.Disconnect("Connection timed out");
                UIController.Instance.OnDisconnectCalled();
            }
        }

        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;

        foreach (var evt in connectionEventsForClient)
        {
            switch (evt.State)
            {
                case ConnectionState.State.Connecting:
                    LoadingScreenUI.Set(LoadingScreenState.Connecting);
                    WorldsManager.ClientConnectingStartTime = (float)SystemAPI.Time.ElapsedTime;
                    UnityEngine.Debug.Log($"[ConnectionEventListener] {evt.Id.Value}:{evt.State.ToString()} on {state.World.Name!}");
                    break;
                case ConnectionState.State.Connected:
                    WorldsManager.ClientConnectingStartTime = -1;
                    LoadingScreenUI.Set(LoadingScreenState.Loading);
                    UnityEngine.Debug.Log($"[ConnectionEventListener] {evt.Id.Value}:{evt.State.ToString()} on {state.World.Name!}");
                    break;
                case ConnectionState.State.Disconnected:
                    WorldsManager.Disconnect($"{evt.DisconnectReason}");
                    UIController.Instance.OnDisconnectCalled();
                    UnityEngine.Debug.Log($"[ConnectionEventListener] {evt.Id.Value}:{evt.State.ToString()} on {state.World.Name!}");
                    break;
            }
        }
    }
}
