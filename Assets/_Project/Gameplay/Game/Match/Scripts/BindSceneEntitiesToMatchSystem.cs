using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Scenes;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct BindSceneEntitiesToMatchSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SceneTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new BindSceneEntitiesToMatchJob
        {
            SceneEntityReferenceLookup = SystemAPI.GetComponentLookup<SceneEntityReference>(true),
            BelongsToMatchLookup = SystemAPI.GetComponentLookup<BelongsToMatch>(true),
            ECB = ecb
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[WithAll(typeof(MatchControlledTag))]
[WithNone(typeof(BelongsToMatch))]
[BurstCompile]
public partial struct BindSceneEntitiesToMatchJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<SceneEntityReference> SceneEntityReferenceLookup;
    [ReadOnly] public ComponentLookup<BelongsToMatch> BelongsToMatchLookup;

    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        SceneTag scenTag,
        Entity entity
    )
    {
        // if scene entity doesn't have SceneEntityReference component it is invalid
        if (!SceneEntityReferenceLookup.HasComponent(scenTag.SceneEntity))
            return;

        // Getting scene reference (with true scene entity)
        SceneEntityReference sceneEntityRef = SceneEntityReferenceLookup[scenTag.SceneEntity];

        // if scene doesn't relate to match skip it
        if (!BelongsToMatchLookup.HasComponent(sceneEntityRef.SceneEntity))
            return;

        BelongsToMatch sceneMatch = BelongsToMatchLookup[sceneEntityRef.SceneEntity];

        ECB.AddComponent(sortKey, entity, new BelongsToMatch
        {
            Entity = sceneMatch.Entity
        });
    }
}