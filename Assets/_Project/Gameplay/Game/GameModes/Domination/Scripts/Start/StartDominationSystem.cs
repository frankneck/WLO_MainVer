using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
// [BurstCompile]
public partial struct StartDominationSystem : ISystem
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

        foreach (var (playerNumbers, matchEntity) in SystemAPI
            .Query<DominationPlayersData>()
            .WithNone<ActiveMatchTag>()
            .WithEntityAccess())
        {
            if (playerNumbers.PlayersNumber >= globalSettings.MinPlayersToStartMatch)
            {
                ecb.AddComponent<ActiveMatchTag>(matchEntity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}