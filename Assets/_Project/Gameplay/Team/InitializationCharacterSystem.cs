using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Rendering;

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
        
        state.RequireForUpdate<PlayerTeam>();
        state.RequireForUpdate(_characterControllerEntityQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        using (var characterControllers = _characterControllerEntityQuery.ToEntityArray(Allocator.Temp))
        {
            foreach (var (playerTeam, playerComponent, playerEntity) in SystemAPI.Query<PlayerTeam, FirstPersonPlayer>().WithAll<NewPlayerTag>().WithEntityAccess())
            {
                var teamColor = playerTeam.Value switch
                {
                   TeamType.Blue => new float4(0, 0, 1, 1),
                   TeamType.Red => new float4(1, 0, 0, 1),
                   _ => new float4(1)
                };

                foreach (var characterController in characterControllers)
                {
                    if (playerComponent.ControlledCharacter == characterController)
                    {
                        var characterRender = SystemAPI.GetComponent<CharacterRender>(characterController);
                        ecb.AddComponent(characterRender.CharacterBody, new URPMaterialPropertyBaseColor { Value = teamColor });
                        ecb.AddComponent(characterRender.CharacterVisor, new URPMaterialPropertyBaseColor { Value = new float4(0, 0, 0, 0)});
                    }        
                }
                
                ecb.RemoveComponent<NewPlayerTag>(playerEntity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }   
}
            