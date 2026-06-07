using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct DefineFinishMatchTickSystem : ISystem
{
    private int m_SimulationTickRate;
 
    public void OnCreate(ref SystemState state)
    {
        m_SimulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameMatchGlobalSettings>();
    }

    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();

        foreach (var (timer, matchEntity) in SystemAPI
            .Query<RefRW<RoundTimer>>()
            .WithAll<FinishingMatchTag, DefineFinishMatchTick>()
            .WithNone<FinishMatchTickDefined>()
            .WithEntityAccess())
        {
            var lifeTimeInTicks = (uint) (globalSettings.TimeAfterFinishingMatch * m_SimulationTickRate);
            
            currentTick.Add(lifeTimeInTicks);
            
            timer.ValueRW.Tick = currentTick;

            UnityEngine.Debug.Log($"[Rounds: DefineFinishMatchTickSystem] Match end tick is {timer.ValueRW.Tick}.");

            ecb.AddComponent<FinishMatchTickDefined>(matchEntity);
            ecb.RemoveComponent<DefineFinishMatchTick>(matchEntity);
        }

        ecb.Playback(state.EntityManager);
    }
}  