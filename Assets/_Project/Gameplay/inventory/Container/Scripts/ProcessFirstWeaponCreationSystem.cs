using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ProcessFirstWeaponCreationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FirstWeaponsSpawnerTag>();
    } 

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Get common spawner data 
        var commonSpawnerEntity = SystemAPI.GetSingletonEntity<FirstWeaponsSpawnerTag>();
        
        var targetEntity = SystemAPI.GetComponent<SpawnTargetEntity>(commonSpawnerEntity);
        var spawnerWeaponLevel = SystemAPI.GetComponent<SpawnerWeaponLevel>(commonSpawnerEntity);
        var quantity = SystemAPI.GetComponent<FirstWeaponsQuantity>(commonSpawnerEntity);

        foreach (var (req, entity) in SystemAPI
            .Query<CreateInitWeapons>()
            .WithEntityAccess())
        {
            for (int i = 0; i < quantity.Value; i++)
            {
                // Create item entity
                Entity itemEntity = targetEntity.Entity;

                // Level assign 
                var assignLevelReq = ecb.CreateEntity();
                ecb.AddComponent(assignLevelReq, new AssignLevelRequest
                {
                    SpawnerEntity = commonSpawnerEntity,
                    SpawnedEntity = itemEntity,
                    Level = spawnerWeaponLevel.Value
                });

                // Add to container
                ecb.AddComponent(itemEntity, new ReadyToAddInContainer
                {
                    ContainerEntity = req.ContainerEntity
                });
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }   
}