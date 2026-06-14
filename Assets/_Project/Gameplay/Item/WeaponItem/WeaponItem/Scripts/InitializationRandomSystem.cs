using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct InitRandomSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        uint seed = 1;

        foreach (var (spellState, entity)
            in SystemAPI.Query<RefRW<StuffSpellState>>()
            .WithAll<NeedsRandomInit>()
            .WithEntityAccess())
        {
            spellState.ValueRW.Random =
                Unity.Mathematics.Random.CreateFromIndex(seed++);

            ecb.RemoveComponent<NeedsRandomInit>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct NeedsRandomInit : IComponentData {}