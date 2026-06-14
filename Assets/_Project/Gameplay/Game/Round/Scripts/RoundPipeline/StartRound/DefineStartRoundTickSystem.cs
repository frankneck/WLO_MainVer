using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DefineStartRoundTickSystem : ISystem
{
    private int m_SimulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameModesPrefabs>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameMatchGlobalSettings>();

        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        var globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();

        foreach (var (roundMatch, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<DefineStartRoundTick, StartingRoundTag>()
            .WithNone<StartRoundTickDefined>()
            .WithEntityAccess())
        {
            Entity matchEntity = roundMatch.Entity;

            if (!SystemAPI.HasComponent<RoundTimer>(matchEntity))
                continue;

            var roundTimer = SystemAPI.GetComponentRW<RoundTimer>(matchEntity);

            // Match settings
            float time = globalSettings.TimeBeforeStartingRound;
            
            var lifeTimeInTicks = (uint) (time * m_SimulationTickRate);
            
            currentTick.Add(lifeTimeInTicks);
            
            roundTimer.ValueRW.Tick = currentTick;

            // UnityEngine.Debug.Log($"[Rounds: DefineStartRoundTickSystem] Start round tick is {roundTimer.ValueRW.Tick}.");

            ecb.RemoveComponent<DefineEndRoundTick>(roundEntity);
            ecb.AddComponent<StartRoundTickDefined>(roundEntity);
        }

        ecb.Playback(state.EntityManager);
    }
} 