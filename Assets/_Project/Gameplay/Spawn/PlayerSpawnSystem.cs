using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct PlayerSpawnSystem : ISystem
{
    private EntityQuery _query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<ServerPlayerInitRequest, ReadySpawn>();
        _query = state.GetEntityQuery(builder);
        
        state.RequireForUpdate(_query);
    }

    public void OnUpdate(ref SystemState state)
    {
        
        if (!SystemAPI.HasSingleton<GhostPrefabs>())
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        GhostPrefabs gameSetup = SystemAPI.GetSingleton<GhostPrefabs>();

        foreach (var (playerInitReq, connectionEntity) in SystemAPI.Query<ServerPlayerInitRequest>().WithAll<ReadySpawn>().WithEntityAccess())
        {
            var requestedTeamType = playerInitReq.TeamValue;
            var requestedPlayerName = playerInitReq.PlayerName;

            float3 spawnPos = new float3(0f, -10f, 0f);
            quaternion spawnRot = quaternion.identity; 

            switch (requestedTeamType)
            {
                case TeamType.Blue:
                    spawnPos = new float3(0f, -10f, -35f);
                    spawnRot = quaternion.LookRotationSafe(new float3(0, 0, 1), math.up());
                    break;
                case TeamType.Red:
                    spawnPos = new float3(0f, -10f, 35f);
                    spawnRot = quaternion.LookRotationSafe(new float3(0, 0, -1), math.up());
                    break;
                default:
                    break;
            }

            Entity characterEntity = ecb.Instantiate(gameSetup.CharacterPrefab);
            Entity playerEntity = ecb.Instantiate(gameSetup.PlayerPrefab);

            ecb.AppendToBuffer(connectionEntity, new LinkedEntityGroup { Value = characterEntity });
            ecb.AppendToBuffer(connectionEntity, new LinkedEntityGroup { Value = playerEntity } );

            LocalTransform localTransform = LocalTransform.FromPositionRotation(spawnPos, spawnRot);
            ecb.SetComponent(characterEntity, localTransform);
            int clientConnectionId = SystemAPI.GetComponent<NetworkId>(connectionEntity).Value;
            ecb.SetComponent(characterEntity, new GameTeam { Value = requestedTeamType });
            ecb.SetComponent(characterEntity, new GhostOwner { NetworkId = clientConnectionId });
            
            ecb.SetComponent(playerEntity, new GhostOwner { NetworkId = clientConnectionId });
            ecb.SetComponent(playerEntity, new GameTeam { Value = requestedTeamType });
            ecb.SetComponent(playerEntity, new PlayerName { Value = requestedPlayerName });

            FirstPersonPlayer player = SystemAPI.GetComponent<FirstPersonPlayer>(gameSetup.PlayerPrefab);
            player.ControlledCharacter = characterEntity;
            ecb.SetComponent(playerEntity, player);

            ecb.AddComponent<NetworkStreamInGame>(connectionEntity);

            ecb.RemoveComponent<ReadySpawn>(connectionEntity);
            UnityEngine.Debug.Log("[Server|Client] Entities (player, controller) has been istantiated.");
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
