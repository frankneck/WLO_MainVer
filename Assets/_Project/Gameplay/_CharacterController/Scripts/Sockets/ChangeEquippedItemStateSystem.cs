using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ShowItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (activeItem, lastActiveItem, entity) in SystemAPI
            .Query<ActiveItem, RefRW<LastActiveItem>>()
            .WithEntityAccess())
        {
            if (activeItem.Entity == lastActiveItem.ValueRW.Entity)
                continue;

            var equipment = SystemAPI.GetBuffer<CharacterEquipment>(entity);

            foreach (var equipped in equipment)
            {
                if (equipped.Item == Entity.Null) 
                    continue;

                if (activeItem.Entity == equipped.Item)
                {
                    SendChangeItemStateRequest(ref ecb, equipped.Item, ItemState.Equiped);
                }
                else
                {
                    SendChangeItemStateRequest(ref ecb, equipped.Item, ItemState.InContainer);
                }
            }

            lastActiveItem.ValueRW.Entity = activeItem.Entity;
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
