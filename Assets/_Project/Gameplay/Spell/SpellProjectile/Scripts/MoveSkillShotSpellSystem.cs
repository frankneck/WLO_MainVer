using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.Rendering;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveSkillShotSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<SpellDirection>, RefRO<ProjectileMoveSpeed>>())
        {
            velocity.ValueRW.Linear = direction.ValueRO.Value * speed.ValueRO.Value;
        }
    }
}