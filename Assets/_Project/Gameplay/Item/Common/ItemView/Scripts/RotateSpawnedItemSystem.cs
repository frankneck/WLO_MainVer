using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct IdleWorldItemViewSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldViewAnimaParameters>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var animationParameters = SystemAPI.GetSingleton<WorldViewAnimaParameters>();

        var time = SystemAPI.Time.ElapsedTime;
        var deltaTime = SystemAPI.Time.DeltaTime;

        var job = new IdleWorldItemViewJob
        {
            Time = time,
            DeltaTime = deltaTime,
            ECB = ecb, 
            AnimParameters = animationParameters
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct IdleWorldItemViewJob : IJobEntity
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public double Time;
    [ReadOnly] public WorldViewAnimaParameters AnimParameters;


    public EntityCommandBuffer.ParallelWriter ECB; 


    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in WorldViewTag tag,
        in ItemViewTransform viewTransform,
        ref LocalTransform transform
    )
    {       
        float offset = (float) math.sin(Time * AnimParameters.RotationSpeed) * AnimParameters.Amplitude;

        var pos = viewTransform.Position; 
        pos.y += offset;
        transform.Position = pos; 

        transform.Rotation = math.mul(math.normalize(transform.Rotation), quaternion.RotateY(AnimParameters.RotationSpeed * DeltaTime));
        transform.Scale = AnimParameters.Scale;
    }
}
