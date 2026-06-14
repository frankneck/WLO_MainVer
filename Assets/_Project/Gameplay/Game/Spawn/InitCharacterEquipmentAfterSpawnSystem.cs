using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
// [BurstCompile]
public partial struct InitCharacterEquipmentAfterSpawnSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (containers, entity) in SystemAPI
            .Query<WithCharacterContainers>()
            .WithAll<NeedToInitEquipmentTag>()
            .WithEntityAccess())
        {
            UnityEngine.Debug.Log("[InitCharacterEquipmentAfterSpawnSystem] Init first");

            var consumableContainer = containers.ConsumableEquipmentContainer;
            var weaponContainer = containers.WeaponEquipmentContainer;

            UpdateContainerVersion(ref ecb, consumableContainer);
            UpdateContainerVersion(ref ecb, weaponContainer);

            ecb.RemoveComponent<NeedToInitEquipmentTag>(entity);
        }

        ecb.Playback(state.EntityManager);
    }

    private void UpdateContainerVersion(
        ref EntityCommandBuffer ecb, 
        Entity container)
    {
        var request = ecb.CreateEntity();
        
        ecb.AddComponent(request, new UpdateContainerVersion 
        { 
            Container = container 
        });
    }
}

public struct NeedToInitEquipmentTag : IComponentData { }