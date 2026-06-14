using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ActivateRoundSystem : ISystem
{
    private int m_SimulationTickRate; 

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();

        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        foreach (var (roundMatch, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<StartRoundTickDefined, StartingRoundTag>()
            .WithNone<ActiveRoundTag>()
            .WithEntityAccess())
        {
            var matchEntity = roundMatch.Entity;

            if (!SystemAPI.HasComponent<RoundTimer>(matchEntity))
                continue;
            
            var roundTimer = SystemAPI.GetComponent<RoundTimer>(matchEntity);

            if (currentTick.IsNewerThan(roundTimer.Tick))
            {
                // UnityEngine.Debug.Log($"[Rounds: ActivateRoundSystem] Round has activated.");

                ecb.AddComponent<ActiveRoundTag>(roundEntity);
                ecb.RemoveComponent<StartingRoundTag>(roundEntity);
                
                // Send requset to define tick
                ecb.AddComponent<DefineEndRoundTick>(roundEntity);
            }
            else
            { 
                // Count seconds to finish rounds
                var ticksToFinishRound = roundTimer.Tick.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                var secondsToFinishRound = (int)(ticksToFinishRound / m_SimulationTickRate) + 1;

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