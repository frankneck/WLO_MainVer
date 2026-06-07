using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Rendering;

/// <summary>
/// Changes color of view entity
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[BurstCompile]
public partial struct InitializationCharacterSystem : ISystem
{
    private EntityQuery _characterControllerEntityQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<FirstPersonCharacterComponent>();
        _characterControllerEntityQuery = state.GetEntityQuery(builder);
        
        state.RequireForUpdate<GameTeam>();
        state.RequireForUpdate(_characterControllerEntityQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        {
            foreach (var (team, view, entity) in SystemAPI
                .Query<GameTeam, LastPlayerCharacterView>()
                .WithAll<NewCharacterPlayerTag>()
                .WithEntityAccess())
            {
                if (!SystemAPI.Exists(view.Entity))
                    continue;

                var teamColor = team.Value switch
                {
                   TeamType.Blue => new float4(0, 0, 1, 1),
                   TeamType.Red => new float4(1, 0, 0, 1),
                   _ => new float4(1)
                };

                ecb.AddComponent(view.Entity, new URPMaterialPropertyBaseColor { Value = teamColor });
                ecb.RemoveComponent<NewCharacterPlayerTag>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }   
}
            