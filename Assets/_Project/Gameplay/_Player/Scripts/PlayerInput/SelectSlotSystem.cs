using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(PredictedFixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct SelectSlotSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        SelectSlotJob jobHandle = new SelectSlotJob();
        state.Dependency = jobHandle.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct SelectSlotJob : IJobEntity
{
    public void Execute(
        ref FirstPersonPlayerCommands playerCommands,
        ref SelectedSlotIndex selectedSlotIndex
    )
    {
        // Character selection
        int currentSelectedIndex = selectedSlotIndex.Value;

        if (playerCommands.HasDirectWeaponSelect)
        {
            currentSelectedIndex = playerCommands.WeaponDirectIndex;
        }
        else
        {
            currentSelectedIndex += playerCommands.WeaponScrollDelta;
        }

        currentSelectedIndex = (currentSelectedIndex + 9) % 9;

        selectedSlotIndex.Value = currentSelectedIndex;
    }
}