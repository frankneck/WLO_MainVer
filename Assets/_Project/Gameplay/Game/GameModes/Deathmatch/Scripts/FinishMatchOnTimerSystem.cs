using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// [BurstCompile]
public partial struct FinishMatchOnTimerSystem : ISystem
{   
    private int m_SimulationtickRate;

    public void OnCreate(ref SystemState state)
    {
        m_SimulationtickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;

        state.RequireForUpdate<NetworkTime>();
    }
    
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        foreach (var (timer, teams, playedRounds, matchEntity) in SystemAPI
            .Query<RefRW<RoundTimer>, RefRW<DeathmatchTeamsData>, RefRW<PlayedRoundsNumber>>()
            .WithAll<FinishingMatchTag, FinishMatchTickDefined>()
            .WithEntityAccess())
        {         
            if (currentTick.IsNewerThan(timer.ValueRW.Tick))
            {
                // null old data 
                teams.ValueRW.BluePlayers = 0;
                teams.ValueRW.RedPlayers = 0;
                teams.ValueRW.BluePlayersWins = 0;
                teams.ValueRW.RedPlayersWins = 0;

                playedRounds.ValueRW.Value = 0;

                // remove old component tags
                ecb.RemoveComponent<FinishingMatchTag>(matchEntity);
                ecb.RemoveComponent<FinishMatchTickDefined>(matchEntity);
                
                // start new match
                ecb.AddComponent<StartingMatchTag>(matchEntity); 

                // send all players that need to choose new command
                foreach (var (belongsToMatch, kdCounter, playerTeam, playerEntity) in SystemAPI
                    .Query<BelongsToMatch, RefRW<KDCounter>, RefRW<GameTeam>>()
                    .WithAll<PlayerTag>()
                    .WithEntityAccess())
                {
                    // different match
                    if (belongsToMatch.Entity != matchEntity)
                        continue;

                    // null old data 
                    playerTeam.ValueRW.Value = TeamType.None;

                    kdCounter.ValueRW.Deaths = 0;
                    kdCounter.ValueRW.Kills = 0;

                    // change current player state
                    PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                        ref ecb, 
                        playerEntity, 
                        PlayerState.SelctingTeam
                    );
                }

                UnityEngine.Debug.Log($"[Rounds: FinishMatchOnTimerSystem] Match has finished because timer is finished.");
            }
            else
            {
                UnityEngine.Debug.Log($"[Rounds: FinishMatchOnTimerSystem] Match is finishing yet.");
                // Count seconds to finish rounds
                var ticksToFinishRound = timer.ValueRW.Tick.TickIndexForValidTick - currentTick.TickIndexForValidTick;
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
} 

// Если таймер окончен -
// Очистить значения 
// Добавить компонент StartinTag