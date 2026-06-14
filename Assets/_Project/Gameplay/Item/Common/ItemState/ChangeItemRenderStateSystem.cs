using Unity.Entities;
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InventoryItemRenderSystem : ISystem
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
                switch (itemState.Value)
                {
                    case ItemState.Inventory :
                        
                        if (!SystemAPI.HasComponent<DisableRendering>(child.Value))
                            ecb.AddComponent<DisableRendering>(child.Value);
                        
                        break;
                    case ItemState.World : 
                        
                        if (SystemAPI.HasComponent<DisableRendering>(child.Value))
                            ecb.RemoveComponent<DisableRendering>(child.Value);
                        
                        break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}