using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct InitCharacterEquipmentAfterSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (containers, entity) in SystemAPI
            .Query<WithCharacterContainers>()
            .WithAll<NeedToFirstUpdateEquipmentContainersVersionTag>()
            .WithEntityAccess())
        {
            var consumableContainer = containers.ConsumableEquipmentContainer;
            var weaponContainer = containers.WeaponEquipmentContainer;

            ContainerVersionHelper.UpdateContainerVersion(ecb, consumableContainer);
            ContainerVersionHelper.UpdateContainerVersion(ecb, weaponContainer);

            ecb.RemoveComponent<NeedToFirstUpdateEquipmentContainersVersionTag>(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct NeedToFirstUpdateEquipmentContainersVersionTag : IComponentData { }