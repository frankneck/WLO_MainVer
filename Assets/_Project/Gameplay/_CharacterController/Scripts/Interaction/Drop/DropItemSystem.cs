using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DropItemSystem : ISystem
{
    private FixedList128Bytes<Entity> m_ItemEntities;

    private float m_ThrowSpeed;
    private float3 m_ThrowUpVector;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_ItemEntities = new FixedList128Bytes<Entity>();

        m_ThrowSpeed = 3f;
        m_ThrowUpVector = new float3(0f, 0.6f, 0f);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (req, entity) in SystemAPI
            .Query<DropItemRequest>()
            .WithEntityAccess())
        {
            // Clear before filling
            m_ItemEntities.Clear();

            // Add original
            m_ItemEntities.Add(req.ItemEntity);
            
            int itemQuantity = req.ItemQuantity;

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

                // Send request to handle 
                Entity removeFromBufferReq = ecb.CreateEntity();
                ecb.AddComponent(removeFromBufferReq, new RemoveDroppedItemFromBuffer
                {
                    ContainerEntity = req.ContainerEntity,
                    IndexInBuffer = req.IndexInBuffer,
                    ItemQuantity = req.ItemQuantity
                });

                ecb.AddComponent<WorldItemTag>(itemEntity);
                
                // Set init transform
                var newTransform = LocalTransform.FromPosition(req.Pos);
                ecb.SetComponent(itemEntity, newTransform);
                
                // Add drop tag
                ecb.AddComponent<DroppedItemTag>(itemEntity);

                // Add drop velocity
                if (SystemAPI.HasComponent<PhysicsVelocity>(itemEntity))
                {
                    ecb.SetComponent(itemEntity, new PhysicsVelocity
                    {
                        Linear = math.forward(req.Rot) * m_ThrowSpeed + m_ThrowUpVector
                    });
                }
                else
                {
                    ecb.AddComponent(itemEntity, new PhysicsVelocity
                    {
                        Linear = math.forward(req.Rot) * m_ThrowSpeed + m_ThrowUpVector
                    });
                }

                ecb.RemoveComponent<ContainerEntityReference>(itemEntity);
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}