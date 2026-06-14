using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DefineFinishRoundTickSystem : ISystem
{
    private int m_SimulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameMatchGlobalSettings>();

        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;

        var globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (roundMatch, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<DefineFinishRoundTick, FinishingRoundTag>()
            .WithNone<FinishRoundTickDefined>()
            .WithEntityAccess())
        {
            Entity matchEntity = roundMatch.Entity;

            if (!SystemAPI.HasComponent<DeathmatchMatchSettings>(matchEntity) || 
                !SystemAPI.HasComponent<RoundTimer>(matchEntity))
            {
                continue;
            }

            var roundFinishTick = SystemAPI.GetComponentRW<RoundTimer>(matchEntity);

            // Match settings
            float roundTime = globalSettings.TimeAfterFinishingRound;

            var lifeTimeInTicks = (uint) (roundTime * m_SimulationTickRate);
            
            currentTick.Add(lifeTimeInTicks);
            
            roundFinishTick.ValueRW.Tick = currentTick;

            UnityEngine.Debug.Log($"[Rounds: DefineFinishRoundTickSystem] Round finish tick is {roundFinishTick.ValueRW.Tick}.");

            ecb.AddComponent<FinishRoundTickDefined>(roundEntity);
            ecb.RemoveComponent<DefineFinishRoundTick>(roundEntity);
        }

        ecb.Playback(state.EntityManager);
    }
} 