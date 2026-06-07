using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ProccessDisconnectSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (rpc, receive, rpcEntity) in SystemAPI.
            Query<DisconnectPlayer, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            UnityEngine.Debug.Log("Disconnect: Receive last rpc");

            if (rpc.Reason.ToString() == "")
                WorldsManager.Disconnect();
            else 
                WorldsManager.Disconnect(rpc.Reason.ToString());

            ecb.DestroyEntity(rpcEntity);
        }
    }
} 