using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ProcessNewRoundSystem : ISystem
{
    private EntityQuery m_PlayerQuery;

    public void OnCreate(ref SystemState state)
    {
        m_PlayerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<FirstPersonPlayer, CurrentPlayerState, GameTeam, BelongsToMatchId>()
            .Build(state.EntityManager);

        state.RequireForUpdate(m_PlayerQuery);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var playerEntities = m_PlayerQuery.ToEntityArray(Allocator.Temp);

        foreach (var (round, match, roundEntity) in SystemAPI
            .Query<RoundTag, BelongsToMatch>()
            .WithAll<NewRoundTag>()
            .WithNone<StartingRoundTag>()
            .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<DeathmatchTeamsData>(match.Entity))
                continue;

            int redPlayers = 0;
            int bluePlayers = 0; 

            foreach (var playerEntity in playerEntities)
            {
                BelongsToMatch playerMatchId = SystemAPI.GetComponent<BelongsToMatch>(playerEntity);

                // Differen match -> skip
                if (playerMatchId.Entity != match.Entity)
                    continue;

                // If player has neccessary status -> create character entity for him 
                if (SystemAPI.HasComponent<AblePlayerToStartRoundTag>(playerEntity))
                {
                    SendCreateRoundCharacterForPlayer(ref ecb, roundEntity, playerEntity);
                }

                var playerTeam = SystemAPI.GetComponent<GameTeam>(playerEntity);

                var playerState = SystemAPI.GetComponent<CurrentPlayerState>(playerEntity);

                if (playerState.Value == PlayerState.Spectating)
                    continue;

                switch (playerTeam.Value)
                {
                    case TeamType.Red:
                        redPlayers++;
                        break;
                    case TeamType.Blue:
                        bluePlayers++;
                        break;
                }
            }

            // Fix number of alive players for each team 
            var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(match.Entity);
            teams.ValueRW.RedPlayersAlive = redPlayers;
            teams.ValueRW.BluePlayersAlive = bluePlayers;

            UnityEngine.Debug.Log($"[Rounds: ProcessNewRoundSystem] Players are assigned.");

            ecb.AddComponent<StartingRoundTag>(roundEntity);
            ecb.AddComponent<DefineStartRoundTick>(roundEntity);
            
            ecb.RemoveComponent<NewRoundTag>(roundEntity);
        }

        ecb.Playback(state.EntityManager);
    }

    private void SendCreateRoundCharacterForPlayer(
        ref EntityCommandBuffer ecb,
        Entity RoundEntity,
        Entity playerEntity)
    {
        var createCharacterForPlayerRequest = ecb.CreateEntity();
        ecb.AddComponent(createCharacterForPlayerRequest, new CreateCharacterForPlayer
        {
            Round = RoundEntity,
            Player = playerEntity
        });
    }   
}

public struct CharacterAssignedTag : IComponentData { }

public struct CreateCharacterForPlayer : IComponentData
{
    public Entity Round;
    public Entity Player;
}
