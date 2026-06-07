using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct SelectItemSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (player, selectedSlotIndex, entity) in SystemAPI
            .Query<RefRW<FirstPersonPlayer>, RefRO<SelectedSlotIndex>>()
            .WithEntityAccess())
        {
            var character = player.ValueRW.ControlledCharacter;

            if (!SystemAPI.HasBuffer<CharacterEquipment>(character)
                || !SystemAPI.HasComponent<ActiveItem>(character))
            {
                continue;
            }

            var currentStuff = SystemAPI.GetComponentRW<ActiveItem>(character);
            var equipment = SystemAPI.GetBuffer<CharacterEquipment>(character);

            if (equipment.Length == 0)
            {
                continue;
            }

            var selectedItem = equipment[selectedSlotIndex.ValueRO.Value].Item;

            if (currentStuff.ValueRW.Entity == selectedItem)
                continue;

            currentStuff.ValueRW.Entity = selectedItem;

            if (selectedItem == Entity.Null)
                continue;

            ecb.SetComponent(currentStuff.ValueRW.Entity, new EquipedBy 
            { 
                Entity =  character 
            });
            
        }
    
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}