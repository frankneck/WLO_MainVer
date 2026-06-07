using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class CloseInventorySystem : SystemBase
{
    private const int MaxPerMessage = 3;

    protected override void OnCreate()
    {
        RequireForUpdate<NetworkStreamConnection>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var localConnection = SystemAPI.GetSingletonEntity<NetworkStreamConnection>();

        foreach (var (_, entity) in SystemAPI
            .Query<CloseInventoryRequest>()
            .WithEntityAccess())
        {
            // Getting commands
            var commands = InventoryController.Instance.CommandBuffer.Commands;
            
            for (int i = 0; i < commands.Count; i += MaxPerMessage)
            {
                var chunk = new FixedList512Bytes<InventoryCommand>();

                for (int j = i; j < i + MaxPerMessage && j < commands.Count; j++)
                {
                    chunk.Add(commands[j]);
                }

                SendRPC(ecb, chunk, localConnection);
            }

            InventoryController.Instance.DestroyInventorySnapshoModel();

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
    }

    private void SendRPC(
        EntityCommandBuffer ecb, 
        FixedList512Bytes<InventoryCommand> chunk,
        Entity localConnection)
    {
        var rpc = ecb.CreateEntity();
        ecb.AddComponent(rpc, new RpcInventoryCommands
        {
            Commands = chunk
        });

        ecb.AddComponent(rpc, new SendRpcCommandRequest
        {
            TargetConnection = localConnection
        });
    }
}

