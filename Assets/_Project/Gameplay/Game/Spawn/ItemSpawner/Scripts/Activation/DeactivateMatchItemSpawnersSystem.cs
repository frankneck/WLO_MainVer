using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct DeactivateMatchItemSpawnersSystem : ISystem
{
    private EntityQuery m_ItemSpawnersQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_ItemSpawnersQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<CollectableItemSpawnerTag>()
            .Build(state.EntityManager);

        state.RequireForUpdate(m_ItemSpawnersQuery);
        state.RequireForUpdate<DeactivateMatchItemSpawners>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var itemSpawners = m_ItemSpawnersQuery.ToEntityArray(Allocator.TempJob);

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new DeactivateMatchItemSpawnersJob
        {
            CurrentSpawnerStateLookup = SystemAPI.GetComponentLookup<CurrentSpawnerState>(true),
            BelongsToMatchLookup = SystemAPI.GetComponentLookup<BelongsToMatch>(true),
            ECB = ecb,
            ItemSpawnersEntityArray = itemSpawners
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        itemSpawners.Dispose(state.Dependency);
    }
}

[BurstCompile]
public partial struct DeactivateMatchItemSpawnersJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CurrentSpawnerState> CurrentSpawnerStateLookup; 
    [ReadOnly] public ComponentLookup<BelongsToMatch> BelongsToMatchLookup;  
    [ReadOnly] public NativeArray<Entity> ItemSpawnersEntityArray;
 
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        DeactivateMatchItemSpawners request,
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

            if (!CurrentSpawnerStateLookup.HasComponent(itemSpawnerEntity))
                continue;
            
            ECB.SetComponent(sortKey, itemSpawnerEntity, new CurrentSpawnerState
            {
                Value = SpawnerState.Disactive
            });
        }

        ECB.DestroyEntity(sortKey, entity);
    }
}