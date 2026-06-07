using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct EquipmentBufferBuildSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (containers, equipmentBuffer, viewState, entity) in SystemAPI
            .Query<WithCharacterContainers, DynamicBuffer<CharacterEquipment>, 
                CharacterEquipmentCashedVersion>()
            .WithEntityAccess())
        {
            var weaponContainer = containers.WeaponEquipmentContainer;
            var consumableContainer = containers.ConsumableEquipmentContainer;

            if (!SystemAPI.HasComponent<ContainerVersion>(weaponContainer) ||
                !SystemAPI.HasComponent<ContainerVersion>(consumableContainer))
            {
                UnityEngine.Debug.Log("[EquipmentBufferBuildSystem] WeaponContainer and consumable contaienr don't have containerVersion");
                continue;
            }

            var weaponVersion = SystemAPI.GetComponent<ContainerVersion>(weaponContainer).Value;
            var consumableVersion = SystemAPI.GetComponent<ContainerVersion>(consumableContainer).Value;

            if (viewState.CachedWeaponVersion == weaponVersion &&
                viewState.CachedConsumableVersion == consumableVersion)
            {
                continue;
            }

            // update cache
            ecb.SetComponent(entity, new CharacterEquipmentCashedVersion
            {
                CachedConsumableVersion = consumableVersion,
                CachedWeaponVersion = weaponVersion
            });

            var weaponBuffer = SystemAPI.GetBuffer<ContainerBuffer>(weaponContainer);
            var consumableBuffer = SystemAPI.GetBuffer<ContainerBuffer>(consumableContainer);
        
            equipmentBuffer.Clear();
            AppendFromBufferToPlayerCharacterEquipmentBuffer(weaponBuffer, equipmentBuffer);
            AppendFromBufferToPlayerCharacterEquipmentBuffer(consumableBuffer, equipmentBuffer);

            UnityEngine.Debug.Log("[EquipmentBufferBuildSystem] Updates containers");
        }

        ecb.Playback(state.EntityManager);
    }

    private void AppendFromBufferToPlayerCharacterEquipmentBuffer(
        DynamicBuffer<ContainerBuffer> source,
        DynamicBuffer<CharacterEquipment> target)
    {
        for (int i = 0; i < source.Length; i++)
        {
            target.Add(new CharacterEquipment
            {
                Item = source[i].ItemEntity,
                Quantity = source[i].Quantity
            });
        }
    }
}