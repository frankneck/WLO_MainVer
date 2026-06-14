using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct CreateCharacterForDominationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GhostPrefabs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        GhostPrefabs ghostPrefabs = SystemAPI.GetSingleton<GhostPrefabs>();

        foreach (var (player, connection, playerName, match, playerEntity) in SystemAPI
            .Query<RefRW<FirstPersonPlayer>, NetworkEntityReference, CharacterName, BelongsToMatch>()
            .WithAll<NeedToSpawnCharacterForPlayer>()
            .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<ActiveMatchTag>(match.Entity))
                continue;

            if (!SystemAPI.HasComponent<NetworkId>(connection.Entity))
                continue;
            
            NetworkId clientConnectionId = SystemAPI.GetComponent<NetworkId>(connection.Entity);
        
            Entity playerCharacterEntity = ecb.Instantiate(ghostPrefabs.CharacterPrefab);
            
            // Setup network connection
            ecb.AddComponent(connection.Entity, new LinkedPlayerCharacter 
            { 
                Player = playerCharacterEntity 
            });
            
            ecb.AppendToBuffer(connection.Entity, new LinkedEntityGroup 
            { 
                Value = playerCharacterEntity 
            });
            
            ecb.SetComponent(connection.Entity, new CommandTarget 
            { 
                targetEntity = playerCharacterEntity 
            });
            
            ecb.SetComponent(playerCharacterEntity, new GhostOwner 
            { 
                NetworkId = clientConnectionId.Value
            });
            
            // Gameplay features 
            ecb.SetComponent(playerCharacterEntity, new GameTeam 
            { 
                Value = TeamType.None 
            });
            
            ecb.SetComponent(playerCharacterEntity, new CharacterName 
            { 
                Value = playerName.Value
            });

            // Container
            ecb.SetComponentEnabled<NeedToCreateContainer>(playerCharacterEntity, true);

            // Add to round
            ecb.AddComponent(playerCharacterEntity, new BelongsToMatch
            {
                Entity = match.Entity
            });

            ecb.SetComponent(playerCharacterEntity, new CharacterOwner 
            { 
                Entity = playerEntity 
            });

            ecb.SetComponent(playerCharacterEntity, new NetworkEntityReference 
            { 
                Entity = connection.Entity 
            });

            ecb.AddComponent(connection.Entity, new PlayerSpawnInfo
            {
                PlayerName = playerName.Value
            });

            SendAssingCharacterRequest(ref ecb, playerEntity, playerCharacterEntity);

            ecb.RemoveComponent<NeedToSpawnCharacterForPlayer>(playerEntity);
        }

        ecb.Playback(state.EntityManager);
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