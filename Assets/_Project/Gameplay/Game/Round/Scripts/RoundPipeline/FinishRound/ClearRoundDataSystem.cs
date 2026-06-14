using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ClearRoundDataSystem : ISystem
{
    [BurstCompile]

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (round, requestEntity) in SystemAPI
            .Query<ClearRoundData>()
            .WithEntityAccess())
        {            
            // Clear round data
            foreach (var (belongsToRound, entity) in SystemAPI
                .Query<BelongsToRound>()
                .WithEntityAccess())
            {
                if (round.RoundEntity == belongsToRound.Entity)
                {
                    ecb.RemoveComponent<BelongsToRound>(entity);
                    ecb.DestroyEntity(entity);
                }

            }
            ecb.AddComponent<RoundCleanupInProgress>(round.RoundEntity);

            ecb.DestroyEntity(requestEntity);
        }

        ecb.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ClearRoundDataSystem))]
[BurstCompile]
public partial struct DestroyCleanRoundsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (match, entity) in
            SystemAPI.Query<BelongsToMatch>()
            .WithAll<RoundCleanupInProgress>()
            .WithEntityAccess())
        {
            bool hasChildren = false;

            foreach (var belongs in SystemAPI.Query<BelongsToRound>())
            {
                if (belongs.Entity == entity)
                {
                    hasChildren = true;
                    break;
                }
            }

            if (!hasChildren)
            {
                ecb.RemoveComponent<BelongsToMatch>(entity);
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}