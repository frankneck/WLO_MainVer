using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Translates KillRequest request into UpdateKDRequest on player entities if it is Current Health less 0.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct HandlePlayerKillSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (killRequest, entity) in SystemAPI
            .Query<KillRequest>()
            .WithEntityAccess())
        {            
            var killer = killRequest.Killer;
            var victim = killRequest.Victim;

            if (!SystemAPI.HasComponent<KDCounter>(killer) || 
                !SystemAPI.HasComponent<KDCounter>(victim))
            {
                continue;
            }

            var dealingCounter = SystemAPI.GetComponentRW<KDCounter>(killer);
            var receivinerKDCounter = SystemAPI.GetComponentRW<KDCounter>(victim);

            dealingCounter.ValueRW.Kills++;
            receivinerKDCounter.ValueRW.Deaths++;

            // GameMode logigs
            if (SystemAPI.HasComponent<DeathmatchTeamsData>(killRequest.MatchEntity))
            {
                // if deathmatch decrease team alive players value
                var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(killRequest.MatchEntity);

                var receivingEntityTeam = SystemAPI.GetComponent<GameTeam>(victim);

                if (receivingEntityTeam.Value == TeamType.Red)
                {
                    teams.ValueRW.RedPlayersAlive--;
                }
                else if (receivingEntityTeam.Value == TeamType.Blue)
                {
                    teams.ValueRW.BluePlayersAlive--;
                }
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

// если это тот, кого убили - игрок - обработать
// если нет - пропустить