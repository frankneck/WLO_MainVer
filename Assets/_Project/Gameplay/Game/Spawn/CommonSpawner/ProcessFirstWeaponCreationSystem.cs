using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ProcessCreatePlayerFirstWeaponsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerFirstWeaponsSpawnerTag>();
    } 

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Get common spawner data 
        var commonSpawnerEntity = SystemAPI.GetSingletonEntity<PlayerFirstWeaponsSpawnerTag>();
        
        var targetEntity = SystemAPI.GetComponent<SpawnerTargetEntity>(commonSpawnerEntity);
        var spawnerWeaponLevel = SystemAPI.GetComponent<SpawnerWeaponLevel>(commonSpawnerEntity);
        var quantity = SystemAPI.GetComponent<PlayerFirstWeaponsSpawnerQuantity>(commonSpawnerEntity);

        foreach (var (req, entity) in SystemAPI
            .Query<SpawnPlayerFirstWeaponsToPutIntoContainer>()
            .WithEntityAccess())
        {
            for (int i = 0; i < quantity.Value; i++)
            {
                // Create item entity
                Entity itemEntity = ecb.Instantiate(targetEntity.PrefabEntity);

                // Level assign 
                var assignLevelReq = ecb.CreateEntity();
                
                ecb.AddComponent(assignLevelReq, new AssignLevelRequest
                {
                    SpawnerEntity = commonSpawnerEntity,
                    SpawnedEntity = itemEntity,
                    Level = spawnerWeaponLevel.Value
                });

                // Add to container
                ecb.AddComponent(itemEntity, new AbleToAddIntoContainer
                {
                    ContainerEntity = req.ContainerEntity
                });

                // what spawned target entity
                ecb.AddComponent(itemEntity, new SpawnerEntityReference
                {
                    Entity = commonSpawnerEntity
                });

                ecb.AddComponent(itemEntity, new ContainerEntityReference
                {
                    Entity = req.ContainerEntity
                });

                ecb.AddComponent(itemEntity, new CurrentPickupMode
                {
                    Value = PickupMode.OnInteract
                });
            }

            ContainerVersionHelper.UpdateContainerVersion(ecb, req.ContainerEntity);

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }   
}