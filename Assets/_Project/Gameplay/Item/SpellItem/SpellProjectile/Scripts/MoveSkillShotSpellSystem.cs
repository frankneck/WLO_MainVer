using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.Rendering;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct MoveSkillShotSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<SpellDirection>();
        state.RequireForUpdate<ProjectileMoveSpeed>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new MoveSkillShotJob();
        state.Dependency = job.ScheduleParallel(state.Dependency);   
    }
}

[BurstCompile]
public partial struct MoveSkillShotJob : IJobEntity
{
    public void Execute(
        ref PhysicsVelocity physicsVelocity,
        in SpellDirection spellDirection,
        in ProjectileMoveSpeed projectileMoveSpeed
    )
    {
        physicsVelocity.Linear = spellDirection.Value * projectileMoveSpeed.Value;
    }
}