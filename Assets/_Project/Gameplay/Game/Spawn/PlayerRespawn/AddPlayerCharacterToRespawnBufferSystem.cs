using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;


/// <summary>
/// Adds player to respawn buffer
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[BurstCompile]
public partial struct AddPlayerCharacterToRespawnBufferSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<RespawnElementBuffer>();
        state.RequireForUpdate<RespawnEntityTag>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        foreach (var (request, entity) in SystemAPI
            .Query<AddCharacterIntoRespawnBuffer>()
            .WithEntityAccess())
        {
            if (!SystemAPI.HasSingleton<RespawnEntityTag>())
            {
                UnityEngine.Debug.Log($"World doesn't have singleton.");
                continue;
            }

            if (!state.EntityManager.Exists(request.CharacterEntity))
            {
                UnityEngine.Debug.Log($"Current сharacter doesn't exists.");
                continue;
            }

            if (!SystemAPI.HasComponent<NetworkEntityReference>(request.CharacterEntity))
            {
                UnityEngine.Debug.Log($"Current entity {request.CharacterEntity} doesn't have NetworkEntityRef");
                continue;
            }

            // Get player connection entity
            var networkEntity = SystemAPI.GetComponent<NetworkEntityReference>(request.CharacterEntity).Entity;
            var respawnEntity = SystemAPI.GetSingletonEntity<RespawnEntityTag>();

            // Add respawn tick value to current tick
            var respawnTickCount = SystemAPI.GetComponent<RespawnTickCount>(respawnEntity).Value;
            var respawnTick = currentTick;
            respawnTick.Add(respawnTickCount);

            UnityEngine.Debug.Log($"Try to add to {respawnEntity}. NetworkEntity is {networkEntity}");

            // Append to respawn buffer to handle it in future 
            ecb.AppendToBuffer(respawnEntity, new RespawnElementBuffer
            {
                NetworkEntity = networkEntity,
                RespawnTick = respawnTick,
                NetworkId = SystemAPI.GetComponent<NetworkId>(networkEntity) 
            });

            ecb.AddComponent<DestroyEntityTag>(request.CharacterEntity);

            ecb.DestroyEntity(entity);
        }
    }
}

public struct PlayerDeadNotificationToClient : IRpcCommand
{
    public Entity Player;
}