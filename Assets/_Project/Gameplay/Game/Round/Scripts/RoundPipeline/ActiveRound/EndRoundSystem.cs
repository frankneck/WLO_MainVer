using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct EndRoundSystem : ISystem
{   
    private int m_SimulationtickRate;

    public void OnCreate(ref SystemState state)
    {
        m_SimulationtickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;

        state.RequireForUpdate<NetworkTime>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        foreach (var (match, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<EndRoundTickDefined, ActiveRoundTag>()
            .WithNone<FinishingRoundTag>()
            .WithEntityAccess())
        {          
            var matchEntity = match.Entity;

            if (!SystemAPI.HasComponent<RoundTimer>(matchEntity) || 
                !SystemAPI.HasComponent<DeathmatchTeamsData>(matchEntity))
            {
                continue;
            }
            
            var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(matchEntity);

            var roundFinishTick = SystemAPI.GetComponentRW<RoundTimer>(matchEntity);

            if (teams.ValueRW.BluePlayersAlive == 0 &&
                teams.ValueRW.RedPlayersAlive == 0)
            {
                // UnityEngine.Debug.Log($"[Rounds: FinishRoundSystem] Round has finished because all players are dead.");
                
                FinishCurrentRound(
                    ref state, 
                    ref ecb, 
                    matchEntity, 
                    roundEntity
                );

                SendClientsToUpdateWinnerScreen(ref ecb, TeamType.None);  
            }
            else if (teams.ValueRW.BluePlayers == 0 
                || teams.ValueRW.BluePlayersAlive == 0)
            {
                // UnityEngine.Debug.Log($"[Rounds: FinishRoundSystem] Round has finished because all blue players are dead.");
                teams.ValueRW.RedPlayersWins++;
                
                FinishCurrentRound(
                    ref state, 
                    ref ecb, 
                    matchEntity, 
                    roundEntity
                );
                
                SendClientsToUpdateWinnerScreen(ref ecb, TeamType.Red);  
            }
            else if (teams.ValueRW.RedPlayers == 0 
                || teams.ValueRW.RedPlayersAlive == 0)
            {
                // UnityEngine.Debug.Log($"[Rounds: FinishRoundSystem] Round has finished because all red players are dead.");
                teams.ValueRW.BluePlayersWins++;
                
                FinishCurrentRound(
                    ref state, 
                    ref ecb, 
                    matchEntity, 
                    roundEntity
                );
                
                SendClientsToUpdateWinnerScreen(ref ecb, TeamType.Blue);
            }
            else if (currentTick.IsNewerThan(roundFinishTick.ValueRW.Tick))
            {
                // Check Timer
                // UnityEngine.Debug.Log($"[Rounds: FinishRoundSystem] Round has finished because timer is finished.");

                if (teams.ValueRW.BluePlayersAlive > teams.ValueRW.RedPlayersAlive)
                {
                    teams.ValueRW.BluePlayersWins++;
                    SendClientsToUpdateWinnerScreen(ref ecb, TeamType.Blue);
                }
                else if (teams.ValueRW.RedPlayersAlive > teams.ValueRW.BluePlayersAlive)
                {
                    teams.ValueRW.RedPlayersWins++;
                    SendClientsToUpdateWinnerScreen(ref ecb, TeamType.Red);
                }
                else
                {
                    SendClientsToUpdateWinnerScreen(ref ecb, TeamType.None);
                }

                FinishCurrentRound(
                    ref state, 
                    ref ecb, 
                    matchEntity, 
                    roundEntity
                );
            }
            else
            { 
                // Count seconds to finish rounds
                var ticksToFinishRound = roundFinishTick.ValueRW.Tick.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                var secondsToFinishRound = (int) (ticksToFinishRound / m_SimulationtickRate) + 1;

                if (SystemAPI.HasComponent<LeftSecondsToFinishRoundTimer>(matchEntity))
                {
                    ecb.SetComponent(matchEntity, new LeftSecondsToFinishRoundTimer
                    {
                        Value = secondsToFinishRound
                    });
                }   
            }
        } 

        ecb.Playback(state.EntityManager);
    }

    private void FinishCurrentRound(
        ref SystemState state,
        ref EntityCommandBuffer ecb,
        Entity matchEntity,
        Entity roundEntity
    )
    {
        // Change player state
        ChangeMatchPlayersStateAndRemoveAssingedTag(
            ref state, 
            ref ecb, 
            matchEntity, 
            PlayerState.PendingFinishRound
        );

        ecb.AddComponent<FinishingRoundTag>(roundEntity);
        ecb.RemoveComponent<ActiveRoundTag>(roundEntity);
        ecb.AddComponent<DefineFinishRoundTick>(roundEntity);  
    }

    private void ChangeMatchPlayersStateAndRemoveAssingedTag(
        ref SystemState state,
        ref EntityCommandBuffer ecb,
        Entity matchEntity,
        PlayerState newPlayerState)
    {
        foreach (var (belongsToMatch, playerState, playerEntity) in SystemAPI
            .Query<BelongsToMatch, CurrentPlayerState>()
            .WithEntityAccess())
        {
            // Other match
            if (belongsToMatch.Entity != matchEntity)
                continue;

            PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                ref ecb, 
                playerEntity, 
                newPlayerState
            );

            ecb.RemoveComponent<CharacterAssignedTag>(playerEntity);
        }
    }

    private void SendClientsToUpdateWinnerScreen(
        ref EntityCommandBuffer ecb,
        TeamType winnerTeam
    )
    {
        UnityEngine.Debug.Log("[SendClientsToUpdateWinnerScreen] Rpc send on clients.");

        var rpcEntity = ecb.CreateEntity();
        ecb.AddComponent(rpcEntity, new UpdateRoundTeamWinnerRpc
        {
            Value = winnerTeam
        });
        ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
    }
}
 
public struct SummarizeRoundRequest : IComponentData
{
    public Entity RoundEntity;
    public Entity MatchEntity;
}
