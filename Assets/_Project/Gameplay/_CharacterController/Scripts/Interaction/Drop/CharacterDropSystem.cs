using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct CharacterDropSystem : ISystem
{
    private EntityArchetype m_DropRequestArchetype;
    private float3 m_Offset;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_DropRequestArchetype = state.EntityManager.CreateArchetype(
            typeof(DropItemRequest)
        );

        m_Offset = new float3(0, 0, 1);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        CharacterDropJob job = new CharacterDropJob
        {
            Offset = m_Offset,
            DropRequestArchetype = m_DropRequestArchetype,

            ContainerEntityReferenceLookup = SystemAPI.GetComponentLookup<ContainerEntityReference>(true),
            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(true),
            
            ECB = ecb
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct CharacterDropJob : IJobEntity
{
    [ReadOnly] public float3 Offset;
    [ReadOnly] public EntityArchetype DropRequestArchetype;
    [ReadOnly] public ComponentLookup<ContainerEntityReference> ContainerEntityReferenceLookup;

    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(
        [EntityIndexInQuery] int sortKey,
        in LocalTransform characterTransform,
        in CharacterOwner characterOwner,
        ref ActiveItem activeItem,
        in FirstPersonCharacterComponent characterComponent,
        in CharacterActionControl characterActionControl,
        in WithCharacterContainers containers
    )
    {
        if (!characterActionControl.Drop)
            return;

        if (activeItem.Entity == Entity.Null || characterOwner.Entity == Entity.Null)
            return;

        if (containers.ConsumableEquipmentContainer == Entity.Null || 
            containers.WeaponEquipmentContainer == Entity.Null)
            return;

        var weaponEquipmentBuffer = ContainerBufferLookup[containers.WeaponEquipmentContainer];
        var consumableEquipmentBuffer = ContainerBufferLookup[containers.ConsumableEquipmentContainer];

        int index = FindActiveItemIndex(activeItem.Entity, weaponEquipmentBuffer, consumableEquipmentBuffer);

        if (ContainerEntityReferenceLookup.HasComponent(activeItem.Entity))
        {
            // Getting owner (where owner could be contaiener entity (e.g. ConsumableContainer, WeaponContaienr etc.))
            ContainerEntityReference container = ContainerEntityReferenceLookup[activeItem.Entity]; 
            
            if (container.Entity == Entity.Null)
                return;

            // To world position
            quaternion cameraRotation = math.mul(characterTransform.Rotation, characterComponent.ViewLocalRotation);
            float3 cameraPosition = characterTransform.Position + new float3(0, 0.4f, 0);
            float3 worldOffset = math.rotate(cameraRotation, Offset);

            Entity dropReq = ECB.CreateEntity(sortKey, DropRequestArchetype);
            ECB.SetComponent(sortKey, dropReq, new DropItemRequest
            {
                ItemEntity = activeItem.Entity,
                ContainerEntity = container.Entity,
                IndexInBuffer = index,
                ItemQuantity = 1,
                Pos = cameraPosition + worldOffset,
                Rot = cameraRotation
            });
        }
    }

    private int FindActiveItemIndex(
        Entity activeItem,
        DynamicBuffer<ContainerBuffer> weaponEquipmentContainerBuffer,
        DynamicBuffer<ContainerBuffer> consumableEquipmentContainerBuffer
    )
    {
        for (int i = 0; i < weaponEquipmentContainerBuffer.Length; i++)
        {
            if (activeItem == weaponEquipmentContainerBuffer[i].ItemEntity)
            {
                return i;
            }
        }

        for (int i = 0; i < consumableEquipmentContainerBuffer.Length; i++)
        {
            if (activeItem == consumableEquipmentContainerBuffer[i].ItemEntity)
            {
                return i;
            }
        }

        return -1;
    }
}

// взять active item
// пройтись по всем буферам 
// если active item совпадает - взять ее и работать с ней уже