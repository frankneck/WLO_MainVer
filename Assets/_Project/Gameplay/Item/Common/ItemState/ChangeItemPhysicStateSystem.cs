using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct InventoryItemPhysicsSystem : ISystem
{
    private float3 m_StaticInventoryItemPosition;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_StaticInventoryItemPosition = new float3(1000f, 1000f, 1000f);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (linkedGroup, itemState, entity) in SystemAPI
            .Query<DynamicBuffer<LinkedEntityGroup>, CurrentItemState>()
            .WithChangeFilter<CurrentItemState>()
            .WithEntityAccess())
        {
            foreach (var child in linkedGroup)
            {
                if (!SystemAPI.HasComponent<Simulate>(child.Value)) 
                    continue;

                switch (itemState.Value)
                {
                    case ItemState.Equiped:
                    case ItemState.Inventory:
                        ecb.SetComponent(entity, LocalTransform.FromPosition(m_StaticInventoryItemPosition));
                        ecb.RemoveComponent<PhysicsVelocity>(entity);
                        break;
                    case ItemState.World: 
                        ecb.AddComponent<PhysicsVelocity>(entity);
                        break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}