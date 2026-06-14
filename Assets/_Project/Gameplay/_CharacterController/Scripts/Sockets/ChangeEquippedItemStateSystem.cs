using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ShowItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (activeItem, cashed, entity) in SystemAPI
            .Query<ActiveItem, RefRW<CashedActiveItem>>()
            .WithEntityAccess())
        {
            // if the same skip update tick
            if (activeItem.Entity == cashed.ValueRW.Entity)
                continue;

            // get player character equipment buffer 
            var equipment = SystemAPI.GetBuffer<CharacterEquipment>(entity);

            // go for buffer
            foreach (var equipped in equipment)
            {
                if (equipped.ItemEntity == Entity.Null)  
                    continue;

                // skip if it's already world item 
                if (SystemAPI.HasComponent<WorldItemTag>(equipped.ItemEntity))
                    continue;

                if (activeItem.Entity == equipped.ItemEntity)
                {
                    // if the same - change item state
                    SendChangeItemStateRequest(ref ecb, equipped.ItemEntity, ItemState.Equiped);
                }
                else
                {
                    // if not - change item state on inventory
                    SendChangeItemStateRequest(ref ecb, equipped.ItemEntity, ItemState.Inventory);
                }
            }

            // update last active
            cashed.ValueRW.Entity = activeItem.Entity;
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void SendChangeItemStateRequest(
        ref EntityCommandBuffer ecb, 
        Entity item, 
        ItemState newState)
    {
        // Change current item state on inventory
        var changeItemStateReq = ecb.CreateEntity();
        ecb.AddComponent(changeItemStateReq, new ChangeCurrentItemState 
        { 
            ItemEntity = item,
            NewState = newState
        });
    }
}
