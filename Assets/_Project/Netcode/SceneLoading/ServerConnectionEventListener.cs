using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerConnectionEventListener : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamDriver>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;
        foreach (var evt in connectionEventsForClient)
        {
            UnityEngine.Debug.Log($"[ConnectionEventListener] {evt.Id.Value}:{evt.State.ToString()}!");
        }
    }
}