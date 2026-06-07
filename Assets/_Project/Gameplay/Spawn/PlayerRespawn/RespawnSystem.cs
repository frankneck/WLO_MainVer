using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class RespawnSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GhostPrefabs>();
        RequireForUpdate<NetworkTime>();
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<RespawnEntityTag>())
        {
            var respawnPrefab = SystemAPI.GetSingleton<GhostPrefabs>().RespawnEntity;
            EntityManager.Instantiate(respawnPrefab);
        }

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        var simulationtickReate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        
        GhostPrefabs ghostPrefabs = SystemAPI.GetSingleton<GhostPrefabs>();

        // Getting buffer with respawn entities. Simulate becaus it's predicted simulation system.
        foreach (var respawnBuffer in SystemAPI
            .Query<DynamicBuffer<RespawnElementBuffer>>()
            .WithAll<RespawnTickCount, Simulate>())
        {
            // To clenup buffer
            var respawnsToCleanup = new NativeList<int>(Allocator.Temp);

            for (int i = respawnBuffer.Length - 1; i >= 0; i--)
            {
                var curRespawn = respawnBuffer[i];

                // Get player
                var playerEntity = SystemAPI.GetComponent<PlayerEntityReference>(curRespawn.NetworkEntity).Entity;  
                
                if (currentTick.Equals(curRespawn.RespawnTick) || currentTick.IsNewerThan(curRespawn.RespawnTick)) // if player respawn
                {
                    // Main Logic
                    // Getting network id and inforamtion about respawning entity
                    var networkId = SystemAPI.GetComponent<NetworkId>(curRespawn.NetworkEntity).Value;
                    var playerSpawnInfo = SystemAPI.GetComponent<PlayerSpawnInfo>(curRespawn.NetworkEntity);
                    
                    var championPrefab = ghostPrefabs.CharacterPrefab;

                    // Instantiating of player character entity
                    var newPlayerCharacterEntity = ecb.Instantiate(championPrefab);

                    // Setup network connection
                    ecb.SetComponent(curRespawn.NetworkEntity, new LinkedPlayerCharacter 
                    { 
                        Player = newPlayerCharacterEntity 
                    });
                    
                    ecb.AppendToBuffer(curRespawn.NetworkEntity, new LinkedEntityGroup 
                    { 
                        Value = newPlayerCharacterEntity 
                    });

                    ecb.SetComponent(curRespawn.NetworkEntity, new CommandTarget 
                    { 
                        targetEntity = newPlayerCharacterEntity 
                    });
                    
                    ecb.SetComponent(newPlayerCharacterEntity, new GhostOwner 
                    { 
                        NetworkId = networkId 
                    });
                    
                    // Gameplay features 
                    ecb.SetComponent(newPlayerCharacterEntity, new GameTeam 
                    { 
                        Value = playerSpawnInfo.Team 
                    });

                    ecb.SetComponent(newPlayerCharacterEntity, new PlayerName 
                    { 
                        Value = playerSpawnInfo.PlayerName 
                    });

                    // ===| Player |===

                    var spawnPos = LocalTransform.FromPositionRotation(playerSpawnInfo.Position, playerSpawnInfo.Rotation);
                    ecb.SetComponent(newPlayerCharacterEntity, spawnPos);
                    
                    ecb.SetComponent(newPlayerCharacterEntity, new NetworkEntityReference 
                    { 
                        Entity = curRespawn.NetworkEntity 
                    });
                    
                    ecb.AddComponent<InvincibilityTag>(newPlayerCharacterEntity);

                    ecb.AddComponent(playerEntity, new AbleToAssignCharacter
                    {
                        CharacterEntity = newPlayerCharacterEntity 
                    });

                    ecb.SetComponent(newPlayerCharacterEntity, new CharacterOwner 
                    { 
                        Entity = playerEntity 
                    });

                    ecb.SetComponentEnabled<NeedToCreateContainer>(newPlayerCharacterEntity, true);

                    // Cleanup
                    respawnBuffer.RemoveAt(i);
                }
                else // if player don't respawn yet 
                {
                    // what ticks to respawn
                    var ticksToRespawn = curRespawn.RespawnTick.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                    
                    // from ticks to seconds
                    var secondsToRespawn = (int) (ticksToRespawn / simulationtickReate); // 61 / 60 = 1
                    
                    if (SystemAPI.HasComponent<LeftSecondsToRespawn>(playerEntity))
                    {
                        ecb.SetComponent(playerEntity, new LeftSecondsToRespawn 
                        { 
                            Value = secondsToRespawn 
                        });
                    }
                }
            }

            respawnsToCleanup.Dispose();
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}