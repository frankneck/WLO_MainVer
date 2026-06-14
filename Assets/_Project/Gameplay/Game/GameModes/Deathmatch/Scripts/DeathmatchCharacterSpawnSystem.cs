using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[BurstCompile]
public partial struct DeathmatchCharacterSpawnSystem : ISystem
{
    private EntityQuery m_SpawnPointsQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_SpawnPointsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TeamSpawnPointTag, PlayerSpawnPointTeam, PlayerSpawnPointOffset, LocalTransform>()
            .Build(state.EntityManager);

        state.RequireForUpdate<GhostPrefabs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        GhostPrefabs ghostPrefabs = SystemAPI.GetSingleton<GhostPrefabs>();

        var spawnPoints = m_SpawnPointsQuery.ToEntityArray(Allocator.TempJob);

        DeathmatchSpawnJob jobHandle = new DeathmatchSpawnJob
        {
            NetworkEntityReferenceLookup = SystemAPI.GetComponentLookup<NetworkEntityReference>(true),
            GameTeamLookup = SystemAPI.GetComponentLookup<GameTeam>(true),
            PlayerNameLookup = SystemAPI.GetComponentLookup<CharacterName>(true),

            NetworkIdLookup = SystemAPI.GetComponentLookup<NetworkId>(true),

            SpawnPointTeamLookup = SystemAPI.GetComponentLookup<PlayerSpawnPointTeam>(true),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            PlayerSpawnPointOffsetLookup = SystemAPI.GetBufferLookup<PlayerSpawnPointOffset>(true),

            TeamSpawnPoints = spawnPoints,            
            GhostPrefabs = ghostPrefabs,
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency); 
        state.Dependency.Complete();
        spawnPoints.Dispose();
    }
}

[BurstCompile]
public partial struct DeathmatchSpawnJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<NetworkEntityReference> NetworkEntityReferenceLookup;
    [ReadOnly] public ComponentLookup<GameTeam> GameTeamLookup;
    [ReadOnly] public ComponentLookup<CharacterName> PlayerNameLookup;
    [ReadOnly] public ComponentLookup<NetworkId> NetworkIdLookup;

    [ReadOnly] public ComponentLookup<PlayerSpawnPointTeam> SpawnPointTeamLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
    [ReadOnly] public BufferLookup<PlayerSpawnPointOffset> PlayerSpawnPointOffsetLookup;

    [ReadOnly] public NativeArray<Entity> TeamSpawnPoints;

    public GhostPrefabs GhostPrefabs;
    public EntityCommandBuffer ECB;
    
    private int m_LastIndex;

    public void Execute(
        CreateCharacterForPlayer request,
        Entity requestEntity)
    {
        Entity playerEntity = request.Player;
        Entity roundEntity = request.Round;

        NetworkEntityReference connectionEntity = NetworkEntityReferenceLookup[playerEntity];
        GameTeam requestedTeamType = GameTeamLookup[playerEntity];
        CharacterName requestedPlayerName = PlayerNameLookup[playerEntity];

        if (!NetworkIdLookup.HasComponent(connectionEntity.Entity))
            return;
        
        NetworkId clientConnectionId = NetworkIdLookup[connectionEntity.Entity];
    
        Entity playerCharacterEntity = ECB.Instantiate(GhostPrefabs.CharacterPrefab);
        
        // Setup network connection
        ECB.AddComponent(connectionEntity.Entity, new LinkedPlayerCharacter 
        { 
            Player = playerCharacterEntity 
        });
        
        ECB.AppendToBuffer(connectionEntity.Entity, new LinkedEntityGroup 
        { 
            Value = playerCharacterEntity 
        });
        
        ECB.SetComponent(connectionEntity.Entity, new CommandTarget 
        { 
            targetEntity = playerCharacterEntity 
        });
        
        ECB.SetComponent(playerCharacterEntity, new GhostOwner 
        { 
            NetworkId = clientConnectionId.Value
        });
        
        // Gameplay features 
        ECB.SetComponent(playerCharacterEntity, new GameTeam 
        { 
            Value = requestedTeamType.Value 
        });
        
        ECB.SetComponent(playerCharacterEntity, new CharacterName 
        { 
            Value = requestedPlayerName.Value
        });

        // Container
        ECB.SetComponentEnabled<NeedToCreateContainer>(playerCharacterEntity, true);

        // Add to round
        ECB.AddComponent(playerCharacterEntity, new BelongsToRound
        {
            Entity = roundEntity
        });

        ECB.SetComponent(playerCharacterEntity, new CharacterOwner 
        { 
            Entity = playerEntity 
        });

        ECB.SetComponent(playerCharacterEntity, new NetworkEntityReference 
        { 
            Entity = connectionEntity.Entity 
        });

        foreach (var teamSpawnPointEntity in TeamSpawnPoints)
        {
            var spawnPointTeam = SpawnPointTeamLookup[teamSpawnPointEntity];
            
            // skip if different teams
            if (spawnPointTeam.Value != requestedTeamType.Value)
                continue;

            var spawnPointTransform = LocalTransformLookup[teamSpawnPointEntity];
            var offsetBuffer = PlayerSpawnPointOffsetLookup[teamSpawnPointEntity];

            float3 offset = offsetBuffer[m_LastIndex].Value;
            float3 newPos = spawnPointTransform.Position + offset;
            LocalTransform newTransform = LocalTransform.FromPositionRotation(newPos, spawnPointTransform.Rotation);

            ECB.SetComponent(playerCharacterEntity, newTransform);
            m_LastIndex = m_LastIndex % offsetBuffer.Length + 1;
        }

        // Send to assign character
        SendAssingCharacterRequest(ref ECB, playerEntity, playerCharacterEntity);

        ECB.DestroyEntity(requestEntity);
    }

    private void SendAssingCharacterRequest(
        ref EntityCommandBuffer ecb,
        Entity playerEntity,
        Entity characterEntity)
    {
        ecb.AddComponent(playerEntity, new AbleToAssignCharacter
        {
            CharacterEntity = characterEntity
        });
    }
}

// Проходим по игрокам
// Берем статус и команду
// Если deathrace - спавн происходит по спавнпойнтам 
// Если deathmatch - спавн происходит в месте с оффсетом