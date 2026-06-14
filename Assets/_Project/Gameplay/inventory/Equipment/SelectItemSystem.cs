using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct SelectItemSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharacterEquipment>();
        state.RequireForUpdate<ActiveItem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var job = new SelectItemJob
        {
            CharacterEquipmentLookup = SystemAPI.GetBufferLookup<CharacterEquipment>(true),
            ActiveItemLookup = SystemAPI.GetComponentLookup<ActiveItem>(true),
            EquipedByLookup = SystemAPI.GetComponentLookup<EquipedBy>(true),
            ECB = ecb
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct SelectItemJob : IJobEntity
{
    [ReadOnly] public BufferLookup<CharacterEquipment> CharacterEquipmentLookup;
    [ReadOnly] public ComponentLookup<ActiveItem> ActiveItemLookup;
    [ReadOnly] public ComponentLookup<EquipedBy> EquipedByLookup;
    public EntityCommandBuffer ECB;

    public void Execute(
        ref FirstPersonPlayer player,
        in SelectedSlotIndex slotIndex,
        Entity entity
    )
    {
        var characterEntity = player.ControlledCharacter;

        if (!CharacterEquipmentLookup.HasBuffer(characterEntity) || 
            !ActiveItemLookup.HasComponent(characterEntity))
        {
            return;
        }

        var equipment = CharacterEquipmentLookup[characterEntity];
        var activeItem = ActiveItemLookup[characterEntity];

        if (equipment.Length == 0 || 
            equipment.Length <= slotIndex.Value)
            return;

        var bufferItem = equipment[slotIndex.Value].ItemEntity;

        // If nothing changed, skip
        if (activeItem.Entity == bufferItem)
            return;

        ECB.SetComponent(characterEntity, new ActiveItem
        { 
            Entity = bufferItem
        });

        if (bufferItem == Entity.Null)
            return;

        if (EquipedByLookup.HasComponent(bufferItem))
        {
            ECB.SetComponent(bufferItem, new EquipedBy
            {
                Entity = characterEntity
            });
        }
    }
}