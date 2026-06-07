using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Type: common System.
/// Description: Adds destory render visual tag to entity.
/// 
/// Flow: CleanupVisualRenderEntitySystem -> DestoryVisualRenderEntitySystem.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct CleanupVisualRenderEntitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp); 

        foreach (var (ghostTarget, entity) in SystemAPI 
            .Query<RefRO<VisualRenderGhostTarget>>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(ghostTarget.ValueRO.Entity))
            {
                ecb.AddComponent<DestroyVisualRenderEntityTag>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
} 

