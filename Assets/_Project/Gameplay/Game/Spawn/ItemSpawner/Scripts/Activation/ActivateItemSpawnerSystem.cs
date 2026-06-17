using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ActivateMatchItemSpawnersSystem : ISystem
{
    private EntityQuery m_ItemSpawnersQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_ItemSpawnersQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<CollectableItemSpawnerTag>()
            .Build(state.EntityManager);

        state.RequireForUpdate(m_ItemSpawnersQuery);
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<ActivateMatchItemSpawners>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var itemSpawners = m_ItemSpawnersQuery.ToEntityArray(Allocator.TempJob);

        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new ActivateMatchItemSpawnersJob
        {
            CurrentSpawnerStateLookup = SystemAPI.GetComponentLookup<CurrentSpawnerState>(true),
            BelongsToMatchLookup = SystemAPI.GetComponentLookup<BelongsToMatch>(true),
            SpawnerTargetTickLookup = SystemAPI.GetComponentLookup<SpawnerTargetTick>(true),
            CurrentTick = currentTick,
            ECB = ecb,
            ItemSpawnersEntityArray = itemSpawners
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        itemSpawners.Dispose(state.Dependency);
    }
}

public partial struct ActivateMatchItemSpawnersJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CurrentSpawnerState> CurrentSpawnerStateLookup; 
    [ReadOnly] public ComponentLookup<BelongsToMatch> BelongsToMatchLookup; 
    [ReadOnly] public ComponentLookup<SpawnerTargetTick> SpawnerTargetTickLookup; 

    [ReadOnly] public NetworkTick CurrentTick;
    [ReadOnly] public NativeArray<Entity> ItemSpawnersEntityArray;

    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in ActivateMatchItemSpawners request,
        Entity entity
    )
    {
        for (int i = 0; i < ItemSpawnersEntityArray.Length; i++)
        {
            var itemSpawnerEntity = ItemSpawnersEntityArray[i];

            if (!BelongsToMatchLookup.HasComponent(itemSpawnerEntity))
                continue;

            var spawnerMatch = BelongsToMatchLookup[itemSpawnerEntity];
            if (request.MatchEntity != spawnerMatch.Entity)
                continue;

            if (!CurrentSpawnerStateLookup.HasComponent(itemSpawnerEntity) ||
                !SpawnerTargetTickLookup.HasComponent(itemSpawnerEntity))
            {
                continue;
            }

            ECB.SetComponent(sortKey, itemSpawnerEntity, new SpawnerTargetTick
            {
                Tick = CurrentTick
            });

            ECB.SetComponent(sortKey, itemSpawnerEntity, new CurrentSpawnerState
            {
                Value = SpawnerState.Active
            });
        }

        ECB.DestroyEntity(sortKey, entity);
    }
}