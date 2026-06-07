using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct AddToContainerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var job = new AddToContainerJob
        {
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
            ECB = ecb
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddToContainerJob : IJobEntity
{
    public BufferLookup<ContainerBuffer> ContainerBufferLookup;
    public EntityCommandBuffer ECB;

    public void Execute(
        in AddToContainer request, 
        Entity entity)
    {
        var containerEntity = request.Container;

        var buffer = ContainerBufferLookup[containerEntity];

        // защита от выхода за границы
        if (request.Index < 0 || request.Index >= buffer.Length)
        {
            ECB.DestroyEntity(entity);
            return;
        }

        buffer[request.Index] = new ContainerBuffer
        {
            ItemEntity = request.Item,
            Quantity = 1
        };

        // Change current item state on inventory
        var changeItemStateReq = ECB.CreateEntity();
        ECB.AddComponent(changeItemStateReq, new ChangeCurrentItemState 
        { 
            ItemEntity = request.Item,
            NewState = ItemState.InContainer
        });

        // удаляем request entity
        ECB.DestroyEntity(entity);
    }
}