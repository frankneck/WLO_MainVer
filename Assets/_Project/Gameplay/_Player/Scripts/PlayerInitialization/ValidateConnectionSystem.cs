using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct ValidateConnectionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var jobHandle = new ValidateConnectionJob
        {
            ECB = ecb
        };

        state.Dependency = jobHandle.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(NetworkStreamInGame))]
public partial struct ValidateConnectionJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in AbleToSnapshotsTag tag1,
        in PlayerEntityReference tag2,
        Entity entity
    )
    {
        ECB.AddComponent<NetworkStreamInGame>(sortKey, entity);
    }
}