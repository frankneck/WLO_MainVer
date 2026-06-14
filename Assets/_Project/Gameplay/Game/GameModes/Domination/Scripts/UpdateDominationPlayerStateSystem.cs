using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct UpdateDominationPlayerStateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach( var (playerState, match) in SystemAPI
            .Query<RefRW<CurrentPlayerState>, BelongsToMatch>()
            .WithAll<DominationEntityTag>())
        {
            Entity matchEntity = match.Entity;
            
            if (SystemAPI.HasComponent<ActiveMatchTag>(matchEntity))
            {
                playerState.ValueRW.Value = PlayerState.Playing;
            }
            else if (SystemAPI.HasComponent<FinishingMatchTag>(matchEntity))
            {
                // If match is finished
                playerState.ValueRW.Value = PlayerState.FinishingMatch;
            }
        }
    }
} 