using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateHudEquipmentSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var hudView = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();
 
        foreach (var (player, selectedSlotIndex, entity) in SystemAPI
            .Query<RefRO<FirstPersonPlayer>, RefRO<SelectedSlotIndex>>()
            .WithAll<GhostOwnerIsLocal>()
            .WithEntityAccess())
        {          
            var character = player.ValueRO.ControlledCharacter; 
            
            if (!SystemAPI.HasBuffer<CharacterEquipment>(character) || 
                !SystemAPI.HasComponent<ActiveItem>(character))
            {
                UnityEngine.Debug.Log("Current player character entity doesn't have equipment buffer and current stuff component.");
                continue;
            }

            var equipment = SystemAPI.GetBuffer<CharacterEquipment>(character);

            hudView.RefreshHudEquipment(EntityManager, entity, equipment);
            hudView.SelectSlot(selectedSlotIndex.ValueRO.Value);
        }  

        ecb.Playback(EntityManager);
    }
}
