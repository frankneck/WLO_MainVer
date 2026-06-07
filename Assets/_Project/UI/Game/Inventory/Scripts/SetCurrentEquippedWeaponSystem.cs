using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct SetCurrentEquippedWeaponSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InventoryView>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var viewEntity = SystemAPI.ManagedAPI.GetSingletonEntity<InventoryView>();
        var view = SystemAPI.ManagedAPI.GetComponent<InventoryView>(viewEntity);

        foreach (var (containers, currentStuff) in SystemAPI
            .Query<RefRW<WithCharacterContainers>, ActiveItem>())
        {
            var equipmentContainer = containers.ValueRW.WeaponEquipmentContainer;  

            if (!SystemAPI.HasBuffer<ContainerBuffer>(equipmentContainer)) 
                continue;

            var equipment = SystemAPI.GetBuffer<ContainerBuffer>(equipmentContainer);

            if (currentStuff.Entity == Entity.Null)
                continue;
            
            for (int i = 0; i < equipment.Length; i++)
            {
                if (equipment[i].ItemEntity == currentStuff.Entity)
                {
                    view.SetCurrentEquipped(i);
                }
                else
                {
                    view.UnsetCurrentEquipped(i);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}