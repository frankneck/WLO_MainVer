using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct GameEntityDeathSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged); 

        foreach (var (_, entity) in SystemAPI
            .Query<PendingDeathTag>()
            .WithAll<Simulate>()
            .WithEntityAccess())
        {       
            UnityEngine.Debug.Log("[PlayerDeathSystem] Update");

            // if pending entity is player
            if (SystemAPI.HasComponent<PlayerCharacterTag>(entity))
            {
                // If entity is player but doesn't have neccessary component -> skip
                if (!SystemAPI.HasComponent<CharacterOwner>(entity) ||
                    !SystemAPI.HasComponent<NetworkEntityReference>(entity) ||
                    !SystemAPI.HasComponent<LastDamager>(entity))
                {
                    continue;
                }
                
                NetworkEntityReference connection = SystemAPI.GetComponent<NetworkEntityReference>(entity);
                LastDamager lastDamager = SystemAPI.GetComponent<LastDamager>(entity);
                
                if (!SystemAPI.HasComponent<CharacterOwner>(entity))
                    continue;

                CharacterOwner dealingDamagePlayer = SystemAPI.GetComponent<CharacterOwner>(lastDamager.Entity);
                CharacterOwner receivingDamagePlayer = SystemAPI.GetComponent<CharacterOwner>(entity);

                if (!SystemAPI.HasComponent<BelongsToMatch>(receivingDamagePlayer.Entity))
                    continue;

                BelongsToMatch match = SystemAPI.GetComponent<BelongsToMatch>(receivingDamagePlayer.Entity);

                if (SystemAPI.HasComponent<DominationMatchTag>(match.Entity))
                {
                    AddToRespawnBuffer(ref ecb, entity);
                } 
                
                PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                    ref ecb, 
                    receivingDamagePlayer.Entity, 
                    PlayerState.Dead
                ); 

                var killRequest = ecb.CreateEntity();

                ecb.AddComponent(killRequest, new KillRequest
                {
                    Killer = dealingDamagePlayer.Entity,
                    Victim = receivingDamagePlayer.Entity,
                    MatchEntity = match.Entity
                });
            }

            // Drop items 
            // if (SystemAPI.HasComponent<WithCharacterContainers>(entity))
            // {
            //     if (!SystemAPI.HasComponent<LocalTransform>(entity))
            //         continue;

            //     var entityTransform = SystemAPI.GetComponent<LocalTransform>(entity);

            //     var containers = SystemAPI.GetComponent<WithCharacterContainers>(entity);

            //     Entity consumableContainer = containers.ConsumableEquipmentContainer;
            //     Entity weaponContainer =  containers.WeaponEquipmentContainer;
            //     Entity inventoryContainer = containers.InventoryContainer;

            //     if (SystemAPI.HasBuffer<ContainerBuffer>(consumableContainer) &&
            //         SystemAPI.HasBuffer<ContainerBuffer>(weaponContainer) && 
            //         SystemAPI.HasBuffer<ContainerBuffer>(inventoryContainer))
            //     {
            //         var consumableBuffer = SystemAPI.GetBuffer<ContainerBuffer>(consumableContainer);
            //         var weaponBuffer = SystemAPI.GetBuffer<ContainerBuffer>(weaponContainer);
            //         var inevntoryBuffer = SystemAPI.GetBuffer<ContainerBuffer>(inventoryContainer);

            //         DropContainerItemsOnDeathFromContainer(ref ecb, entityTransform, consumableBuffer);
            //         DropContainerItemsOnDeathFromContainer(ref ecb, entityTransform, weaponBuffer);
            //         DropContainerItemsOnDeathFromContainer(ref ecb, entityTransform, inevntoryBuffer);
            //     }
            // }
            
            ecb.RemoveComponent<PendingDeathTag>(entity);
            ecb.AddComponent<DestroyEntityTag>(entity);
        }
    }

    private void DropContainerItemsOnDeathFromContainer(
        ref EntityCommandBuffer ecb,
        LocalTransform entityTransform,
        DynamicBuffer<ContainerBuffer> containerBuffer)
    {
        foreach (var e in containerBuffer)
        {
            if (e.ItemEntity == Entity.Null)
                continue;

            Entity dropReq = ecb.CreateEntity();
            ecb.AddComponent(dropReq, new DropItemRequest
            {
                Pos = entityTransform.Position,
                Rot = entityTransform.Rotation,
                ItemEntity = e.ItemEntity,
                Quantity = e.Quantity
            });

            ecb.RemoveComponent<ItemOwner>(e.ItemEntity);
        }
    }

    private void AddToRespawnBuffer(
        ref EntityCommandBuffer ecb,
        Entity characterEntity)
    {
        var addToRespawnBufferRequest = ecb.CreateEntity();
        ecb.AddComponent(addToRespawnBufferRequest, new AddCharacterIntoRespawnBuffer
        {
            CharacterEntity = characterEntity
        });
    }
}

public struct DropItemRequest : IComponentData
{
    public float3 Pos;
    public quaternion Rot;
    public int Quantity;
    public Entity ItemEntity; 
} 