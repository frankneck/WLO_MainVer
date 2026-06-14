using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct FinishRoundOnTimerSystem : ISystem
{
    private int m_SimulationTickRate;
 
    public void OnCreate(ref SystemState state)
    {
        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameMatchGlobalSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        foreach (var (match, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<FinishRoundTickDefined, FinishingRoundTag>()
            .WithNone<FinishedRoundTag>()
            .WithEntityAccess())
        {          
            var matchEntity = match.Entity;

            if (!SystemAPI.HasComponent<RoundTimer>(matchEntity))
                continue;

            var roundFinishTick = SystemAPI.GetComponentRW<RoundTimer>(matchEntity);

            if (currentTick.IsNewerThan(roundFinishTick.ValueRW.Tick))
            {
                UnityEngine.Debug.Log($"[Rounds: FinishRoundOnTimerSystem] Current round is fully finished because time is done.");
                SummarizeRound(ref ecb, roundEntity, matchEntity);
                ecb.AddComponent<FinishedRoundTag>(roundEntity);
            }
            else
            { 
                // Count seconds to finish rounds
                var ticksToFinishRound = roundFinishTick.ValueRW.Tick.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                var secondsToFinishRound = (int) (ticksToFinishRound / m_SimulationTickRate) + 1;

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
    
    private void SummarizeRound(
        ref EntityCommandBuffer ecb,
        Entity roundEntity,
        Entity matchEntity
    )
    {
        var request = ecb.CreateEntity();
        ecb.AddComponent(request, new SummarizeRoundRequest
        {
            RoundEntity = roundEntity,
            MatchEntity = matchEntity
        });
    }
}