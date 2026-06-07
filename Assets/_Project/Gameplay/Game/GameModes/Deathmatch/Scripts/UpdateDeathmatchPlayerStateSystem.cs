using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct UpdateDeathmatchPlayerStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach( var (playerComponent, playerState, match) in SystemAPI
            .Query<FirstPersonPlayer, RefRW<CurrentPlayerState>, BelongsToMatch>()
            .WithAll<DeathmatchEntityTag>())
        {
            Entity characterEntity = playerComponent.ControlledCharacter;

            if (!SystemAPI.HasComponent<BelongsToRound>(characterEntity))
                continue;

            Entity matchEntity = match.Entity;

            Entity roundEntity = SystemAPI.GetComponent<BelongsToRound>(characterEntity).Entity;
            
            // If match is active
            if (SystemAPI.HasComponent<ActiveMatchTag>(matchEntity))
            {
                if (SystemAPI.HasComponent<StartingRoundTag>(roundEntity))
                {
                    playerState.ValueRW.Value = PlayerState.PendingStartRound;
                }   
                else if (SystemAPI.HasComponent<ActiveRoundTag>(roundEntity))
                {
                    playerState.ValueRW.Value = PlayerState.Playing;
                }   
                else if (SystemAPI.HasComponent<FinishingRoundTag>(roundEntity))
                {
                    playerState.ValueRW.Value = PlayerState.PendingFinishRound;
                }
            }
            else if (SystemAPI.HasComponent<FinishingMatchTag>(matchEntity))
            {
                // If match is finished
                playerState.ValueRW.Value = PlayerState.FinishingMatch;
            }
        }
    }
}  