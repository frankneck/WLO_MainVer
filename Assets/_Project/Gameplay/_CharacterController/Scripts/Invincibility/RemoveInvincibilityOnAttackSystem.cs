using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

// [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct RemoveInvincibilityOnAttackSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (_, entity) in SystemAPI.Query<RefRO<SelectedSpellToSpawn>>().WithEntityAccess())
        {
            // Remove Invincibility if player attacks
            if (SystemAPI.HasComponent<InvincibilityTag>(entity))
            {
                ecb.RemoveComponent<InvincibilityTag>(entity);
            }

            // Apply default modifier
            if (SystemAPI.HasComponent<DamageMultiplier>(entity))
            {
                var modifier = SystemAPI.GetComponent<DamageMultiplier>(entity);
                modifier.Multiplier = 1f;
                ecb.SetComponent(entity, modifier);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}