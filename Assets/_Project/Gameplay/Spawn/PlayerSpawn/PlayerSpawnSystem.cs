using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerSpawnSystem : ISystem
{
    private EntityQuery m_MatchEntityQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {        
        m_MatchEntityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<MatchTag>().Build(state.EntityManager);
        
        state.RequireForUpdate(m_MatchEntityQuery);
        state.RequireForUpdate<GhostPrefabs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        GhostPrefabs gameSetup = SystemAPI.GetSingleton<GhostPrefabs>();

        var matchEntities = m_MatchEntityQuery.ToEntityArray(Allocator.Temp);  

        foreach (var (initReqeuest, connectionEntity) in SystemAPI
            .Query<ServerPlayerInitRequest>()
            .WithNone<PlayerSpawned>()
            .WithEntityAccess())
        {
            Entity matchEntity = Entity.Null;
            bool isMatchFounded = false; 

            foreach (var e in matchEntities)
            {
                if (SystemAPI.HasComponent<FinishingMatchTag>(e))
                    continue;
                
                bool isDeathrace = SystemAPI.HasComponent<DominationMatchTag>(e);
                bool isDeathmatch = SystemAPI.HasComponent<DeathmatchMatchTag>(e);

                if (initReqeuest.GameMode == GameMode.Domination && isDeathrace)
                {
                    matchEntity = e;
                    isMatchFounded = true;
                    break;
                }
                else if (initReqeuest.GameMode == GameMode.Deathmatch && isDeathmatch)
                {
                    matchEntity = e;
                    isMatchFounded = true;
                    break;
                }
            }

            if (!isMatchFounded)
            {
                SendDisconnectRequest(ref ecb, connectionEntity);
            }

            // TODO: RPC failed connection
            if (matchEntity == Entity.Null)
            {
                UnityEngine.Debug.Log("[PlayerSpawnSystem] Attention! The needed match hasn't found. Try again.");
                continue;
            }

            var requestedTeamType = initReqeuest.TeamValue;
            var requestedPlayerName = initReqeuest.PlayerName;
            
            int clientConnectionId = SystemAPI.GetComponent<NetworkId>(connectionEntity).Value;
            
            Entity playerEntity = ecb.Instantiate(gameSetup.PlayerPrefab);
            
            // Setup network connection
            ecb.AppendToBuffer(connectionEntity, new LinkedEntityGroup 
            { 
                Value = playerEntity 
            });
            
            ecb.SetComponent(playerEntity, new GhostOwner 
            { 
                NetworkId = clientConnectionId 
            });

            ecb.AddComponent(connectionEntity, new PlayerEntityReference 
            { 
                Entity = playerEntity 
            });

            // Gameplay features 
            ecb.SetComponent(playerEntity, new PlayerName 
            { 
                Value = requestedPlayerName 
            });

            ecb.SetComponent(playerEntity, new GameTeam 
            { 
                Value = requestedTeamType
            });

            ecb.SetComponent(playerEntity, new BelongsToMatch
            {
                Entity = matchEntity
            });

            ecb.SetComponent(playerEntity, new NetworkEntityReference 
            { 
                Entity = connectionEntity 
            });

            switch (initReqeuest.GameMode)
            {
                case GameMode.Deathmatch :
                    ecb.AddComponent<DeathmatchEntityTag>(playerEntity);
                    PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                        ref ecb, 
                        playerEntity, 
                        PlayerState.SelctingTeam
                    );
                    break;
                case GameMode.Domination :
                    var domninationData = SystemAPI.GetComponentRW<DominationPlayersData>(matchEntity);
                    domninationData.ValueRW.PlayersNumber++;
                    ecb.AddComponent<DominationEntityTag>(playerEntity);
                    ecb.AddComponent<NeedToSpawnCharacterForPlayer>(playerEntity);
                    PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                        ref ecb, 
                        playerEntity, 
                        PlayerState.PendingStartMatch
                    );
                    break;
            }

            ecb.AddComponent<PlayerSpawned>(connectionEntity);
            ecb.AddComponent<NetworkStreamInGame>(connectionEntity);

            ecb.RemoveComponent<ServerPlayerInitRequest>(connectionEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void SendDisconnectRequest(
        ref EntityCommandBuffer ecb,
        Entity connectionEntity
    )
    {
        var disconnectRpc = ecb.CreateEntity();
        ecb.AddComponent(disconnectRpc, new DisconnectPlayer 
        { 
            Reason = "Match doesn't founded. Try again."
        });

        ecb.AddComponent(disconnectRpc, new SendRpcCommandRequest 
        { 
            TargetConnection = connectionEntity 
        });
    }
}

/// <summary>
/// Force to spawn playable character for player
/// </summary>
public struct NeedToSpawnCharacterForPlayer : IComponentData { }

public struct DeathmatchEntityTag : IComponentData { }

public struct DominationEntityTag : IComponentData { }

public struct AblePlayerToStartRoundTag : IComponentData { }   

public struct PlayerSpawned : IComponentData { }


public partial struct DisconnectPlayer : IRpcCommand
{
    public FixedString128Bytes Reason;
}
