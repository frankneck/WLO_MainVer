using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct CreateCollectableItemBySpawnerSystem : ISystem
{
    private int _tickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        _tickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;    
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        var job = new CollectableItemSpawnerJob
        {
            WithWeaponContainerLookup = SystemAPI.GetComponentLookup<WithWeaponContainer>(true),
            SpawnerWeaponLevelLookup = SystemAPI.GetComponentLookup<SpawnerWeaponLevel>(true),
            CurrentRoundEntityReferenceLookup = SystemAPI.GetComponentLookup<CurrentRoundEntityReference>(true),
            TickRate = _tickRate,
            CurrentTick = currentTick,
            ECB = ecb  
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
} 

[BurstCompile]
public partial struct CollectableItemSpawnerJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<WithWeaponContainer> WithWeaponContainerLookup;
    [ReadOnly] public ComponentLookup<SpawnerWeaponLevel> SpawnerWeaponLevelLookup;
    [ReadOnly] public ComponentLookup<CurrentRoundEntityReference> CurrentRoundEntityReferenceLookup;
    [ReadOnly] public NetworkTick CurrentTick;
    [ReadOnly] public int TickRate;
    
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in SpawnPointTransform spawnPoint,
        in SpawnerTargetEntity targetEntity,
        in BelongsToMatch spawnerMatch,
        ref SpawnerTargetTick targetTick,
        ref CurrentSpawnerState spawnerState,
        in SpawnerCooldown cooldown,
        Entity entity
    )
    {
        if (spawnerState.Value == SpawnerState.Active)
        {
            if (CurrentTick.IsNewerThan(targetTick.Tick))
            {
                var itemEntity = ECB.Instantiate(sortKey, targetEntity.PrefabEntity);

                // what spawned target entity
                ECB.AddComponent(sortKey, itemEntity, new SpawnerEntityReference
                {
                    Entity = entity
                });

                // We can pickup item by interaction
                ECB.SetComponent(sortKey, itemEntity, new CurrentPickupMode 
                {
                    Value = PickupMode.OnInteract
                });

                ECB.SetComponent(sortKey, itemEntity, new CurrentItemState 
                { 
                    Value = ItemState.World 
                });

                ECB.AddComponent<WorldItemTag>(sortKey, itemEntity);

                // Set transform for spawned item
                LocalTransform newTransform = LocalTransform.FromPositionRotation(spawnPoint.Position, spawnPoint.Rotation);
                ECB.SetComponent(sortKey, itemEntity, newTransform);
                
                // Turn off spawner
                spawnerState.Value = SpawnerState.Disactive;
                
                // if match has round entity reference 
                if (CurrentRoundEntityReferenceLookup.TryGetComponent(spawnerMatch.Entity, out var roundEntityRef))
                {
                    ECB.AddComponent(sortKey, itemEntity, new BelongsToRound
                    {
                        Entity = roundEntityRef.Entity
                    });
                }

                if (!SpawnerWeaponLevelLookup.TryGetComponent(entity, out var spawnerWeaponLevel))
                    return;

                // Level 
                var assignLevelReq = ECB.CreateEntity(sortKey);
                ECB.AddComponent(sortKey, assignLevelReq, new AssignLevelRequest
                {
                    SpawnerEntity = entity,
                    SpawnedEntity = itemEntity,
                    Level = spawnerWeaponLevel.Value
                });
            }
        }
        else
        {
            var cooldownInTicks = (uint)(TickRate * cooldown.Value);
            var nexTick = CurrentTick;
            nexTick.Add(cooldownInTicks);
            targetTick.Tick = nexTick;
        }
    }
}