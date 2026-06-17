using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct StartRoundSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameModesPrefabs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        Entity roundPrefabEntity = SystemAPI.GetSingleton<GameModesPrefabs>().RoundEntityPrefab;

        foreach (var (settings, playedRoundsNumber, matchEntity) in SystemAPI
            .Query<DeathmatchMatchSettings, RefRW<PlayedRoundsNumber>>()
            .WithAll<ActiveMatchTag>()
            .WithNone<CurrentRoundEntityReference>()
            .WithEntityAccess())
        {
            if (settings.RoundsNumber <= 0)
            {
                // UnityEngine.Debug.LogWarning($"[Rounds: StartNewRoundSystem] Rounds number equals invalid value.");
                continue;
            }

            // UnityEngine.Debug.Log($"[Rounds: StartNewRoundSystem] Round created.");

            Entity roundEntity = ecb.Instantiate(roundPrefabEntity);

            ecb.AddComponent(matchEntity, new CurrentRoundEntityReference
            {
                Entity = roundEntity
            });

            ecb.SetComponent(roundEntity, new BelongsToMatch
            {
                Entity = matchEntity
            });

            playedRoundsNumber.ValueRW.Value++;

            // Start to initialize
            ecb.AddComponent<NewRoundTag>(roundEntity);
        }
     
        ecb.Playback(state.EntityManager);
    }
}

