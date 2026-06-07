using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InventoryItemPhysicsSystem : ISystem
{
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
                if (!SystemAPI.HasComponent<Simulate>(child.Value)) continue;

                switch (itemState.Value)
                {
                    case ItemState.InContainer :
                        var pos = LocalTransform.FromPosition(new float3(1000f, 1000f, 1000f));
                        ecb.SetComponent(entity, pos);
                        break;
                    case ItemState.World : 
                        break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

// TODO: Drop system 