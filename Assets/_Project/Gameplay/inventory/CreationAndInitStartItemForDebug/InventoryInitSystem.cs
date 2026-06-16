
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Rendering;

/// <summary>
/// Moves created items to player character backpack
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct MoveCreatedItemsToInventorySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AbleToAddIntoContainer>();    
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var moveJob = new MoveCreatedItemsToInventoryJob
        {
            CurrentItemIdLookup = SystemAPI.GetComponentLookup<CurrentItemId>(true),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
            ECB = ecb
        };

        state.Dependency = moveJob.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct MoveCreatedItemsToInventoryJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<CurrentItemId> CurrentItemIdLookup; 
    public BufferLookup<ContainerBuffer> ContainerBufferLookup;
    public EntityCommandBuffer ECB;

    public void Execute( 
        AbleToAddIntoContainer req,
        Entity reqEntity)
    {
        if (ContainerBufferLookup.TryGetBuffer(req.ContainerEntity, out var containerBuffer))
        {
            AddItemEntityToBuffer(ref containerBuffer, ref reqEntity);
        }
        
        ECB.RemoveComponent<AbleToAddIntoContainer>(reqEntity);
    }

    private void AddItemEntityToBuffer(
        ref DynamicBuffer<ContainerBuffer> buffer,
        ref Entity entity)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].ItemEntity != Entity.Null)
                continue;

            buffer[i] = new ContainerBuffer 
            { 
                ItemEntity = entity,
                Quantity = 1 
            };

            break;
        }
    }
}