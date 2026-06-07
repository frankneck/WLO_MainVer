using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
// [BurstCompile]
public partial struct StartDeathmatchSystem : ISystem
{
    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameMatchGlobalSettings>();
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        GameMatchGlobalSettings globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();

        foreach (var (matchTeams, matchEntity) in SystemAPI
            .Query<DeathmatchTeamsData>()
            .WithAll<StartingMatchTag>()
            .WithNone<ActiveMatchTag>()
            .WithEntityAccess())
        {
            bool allPlayerIsPending = true;

            foreach (var (playerMatch, playerState) in SystemAPI
                .Query<BelongsToMatch, CurrentPlayerState>())
            {
                // Different match
                if (matchEntity != playerMatch.Entity)
                    continue;
            
                if (playerState.Value != PlayerState.PendingStartMatch)
                {
                    allPlayerIsPending = false;
                }
            }

            if (allPlayerIsPending && 
                matchTeams.RedPlayers >= globalSettings.MinPlayersPerTeamToStartDeathmatch &&
                matchTeams.BluePlayers >= globalSettings.MinPlayersPerTeamToStartDeathmatch)
            {
                ecb.AddComponent<ActiveMatchTag>(matchEntity);
                ecb.RemoveComponent<StartingMatchTag>(matchEntity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

// Пройтись по игрками 
// Получить статус
// Если все ожидают -> проверить что 