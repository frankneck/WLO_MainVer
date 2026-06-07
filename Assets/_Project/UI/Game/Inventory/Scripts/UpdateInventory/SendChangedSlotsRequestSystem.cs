using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using System.Linq;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientChangeSlotRequestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<NetworkStreamConnection>();
        state.RequireForUpdate<SpellsInWeaponBuffer>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var connection = SystemAPI.GetSingletonEntity<NetworkStreamConnection>();

        foreach (var (request, entity) in SystemAPI
            .Query<ClientSlotsArrayChanged>()
            .WithEntityAccess())
        {
            Debug.Log(
                $"[CLIENT] Sending ChangeSlot RPC | " +
                $"SourceIndex={request.SourceIndex} " +
                $"SourceType={request.SourceType} " +
                $"TargetIndex={request.TargetIndex} " +
                $"TargetType={request.TargetType} " +
                $"Connection={connection}" + 
                $"SourceOwner={request.SourceOwner} " +
                $"TargetOwner={request.TargetOwner}"
            );

            var reqEntity = ecb.CreateEntity();
            ecb.AddComponent(reqEntity, new SendRpcCommandRequest { TargetConnection = connection });
            ecb.AddComponent(reqEntity, new ClientChangeItemPlace
            {
                SourceIndex = request.SourceIndex,
                SourceType = request.SourceType,
                SourceOwner = request.SourceOwner,
                TargetIndex = request.TargetIndex,
                TargetType = request.TargetType,
                TargetOwner = request.TargetOwner
            });
            
            ecb.RemoveComponent<ClientSlotsArrayChanged>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}