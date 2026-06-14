using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[BurstCompile]
public partial struct CleanupDetachedViewSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (attachedToCharacter, entity) in SystemAPI
            .Query<RefRO<AttachedToCharacter>>()
            .WithAny<FirstPersonViewTag, ThirdPersonViewTag>()
            .WithEntityAccess())
        {
            if (attachedToCharacter.ValueRO.Entity == Entity.Null ||
                !SystemAPI.Exists(attachedToCharacter.ValueRO.Entity))
            {
                ecb.AddComponent<DestroyEntityTag>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}  