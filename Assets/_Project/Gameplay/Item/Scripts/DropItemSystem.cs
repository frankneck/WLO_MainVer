using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DropItemSystem : ISystem
{
    private FixedList128Bytes<Entity> m_ItemEntities;
    private FixedList128Bytes<float3> m_Offsets;
    private int m_LastIndex;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_ItemEntities = new FixedList128Bytes<Entity>();
        
        m_Offsets = new FixedList128Bytes<float3>
        {
            new float3(-1f, 1f, -1f),
            new float3(0f, 1f, -1f),
            new float3(1f, 1f, -1f),
            new float3(1f, 1f, 0f),
            new float3(1f, 1f, 1f),
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        int count = 0;

        foreach (var (req, entity) in SystemAPI
            .Query<DropItemRequest>()
            .WithEntityAccess())
        {
            // Clear before filling
            m_ItemEntities.Clear();

            // Add original
            m_ItemEntities.Add(req.ItemEntity);
            
            int itemQuantity = req.Quantity;

            if (itemQuantity > 1)
            {
                for (int i = 1; i < itemQuantity; i++)
                {
                    var replicatedItem = ecb.Instantiate(req.ItemEntity);
                    m_ItemEntities.Add(replicatedItem);
                }
            }

            foreach(var itemEntity in m_ItemEntities)
            {
                var changeItemState = ecb.CreateEntity();
                
                ecb.AddComponent(changeItemState, new ChangeCurrentItemState
                {
                    NewState = ItemState.World,
                    ItemEntity = itemEntity
                });

                var newPos = req.Pos + m_Offsets[m_LastIndex];

                var newTransform = LocalTransform.FromPositionRotation(newPos, req.Rot);

                ecb.SetComponent(itemEntity, newTransform);

                m_LastIndex = count % m_Offsets.Length + 1;
            }

            m_LastIndex = count % m_Offsets.Length + 1;

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

// Получить кол-во. Если больше одного - инстациировать оставшую часть
// Поменят