using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct DefineEndRoundTickSystem : ISystem
{
    private int m_SimulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }   

    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (roundMatch, roundEntity) in SystemAPI
            .Query<BelongsToMatch>()
            .WithAll<DefineEndRoundTick, ActiveRoundTag>()
            .WithNone<EndRoundTickDefined>()
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
            float roundTime = SystemAPI.GetComponent<DeathmatchMatchSettings>(matchEntity).RoundTime;

            var lifeTimeInTicks = (uint) (roundTime * m_SimulationTickRate);
            
            currentTick.Add(lifeTimeInTicks);
            
            roundFinishTick.ValueRW.Tick = currentTick;

            UnityEngine.Debug.Log($"[Rounds: DefineEndRoundTickSystem] Round end tick is {roundFinishTick.ValueRW.Tick}.");

            ecb.AddComponent<EndRoundTickDefined>(roundEntity);
            ecb.RemoveComponent<DefineEndRoundTick>(roundEntity);
        }

        ecb.Playback(state.EntityManager);
    }
} 