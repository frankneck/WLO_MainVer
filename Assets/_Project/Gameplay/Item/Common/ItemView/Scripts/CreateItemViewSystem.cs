using System.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct CreateItemViewSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (lastView, viewPrefabs, currentState, equiped, itemEntity) in SystemAPI
            .Query<LastViewEntity, ItemViews, CurrentItemState, RefRO<EquipedBy>>()
            .WithChangeFilter<CurrentItemState>()
            .WithEntityAccess())
        {
            // Attention! Character for currentItemState world equals null! Pay attention!
            Entity character = equiped.ValueRO.Entity;

            if (SystemAPI.Exists(lastView.Entity))
            {
                ecb.AddComponent<DestroyEntityTag>(lastView.Entity);
            }

            bool isLocal = SystemAPI.HasComponent<LocalCharacterTag>(equiped.ValueRO.Entity);
            Entity socketEntity;

            switch (currentState.Value)
            {
                case ItemState.World:
                    var worldView = ecb.Instantiate(viewPrefabs.WorldViewPrefab);
                    
                    // Tag
                    ecb.AddComponent<WorldViewTag>(worldView);
                    
                    // Data
                    ecb.AddComponent(worldView, new ItemViewOwner { Entity = itemEntity });
                    ecb.SetComponent(itemEntity, new LastViewEntity { Entity = worldView });
                    break;
                case ItemState.Equiped:
                    if (isLocal)
                    {
                        if (!SystemAPI.HasComponent<ThirdPersonCharacterSocket>(character))
                            continue;

                        var fpView = ecb.Instantiate(viewPrefabs.FirstPersonViewPrefab);
    
                        // Tag
                        ecb.AddComponent<FirstPersonViewTag>(fpView);

                        // Transform
                        socketEntity = SystemAPI.GetComponent<FirstPersonCharacterSocket>(character).Entity;
                        ecb.AddComponent(fpView, new Parent { Value = socketEntity });
                        ecb.SetComponent(fpView, LocalTransform.Identity);

                        // Data
                        ecb.AddComponent(fpView, new AttachedToCharacter { Entity = character });
                        ecb.AddComponent(fpView, new ItemViewOwner { Entity = itemEntity });
                        ecb.SetComponent(itemEntity, new LastViewEntity { Entity = fpView });
                    }
                    else
                    {
                        if (!SystemAPI.HasComponent<FirstPersonCharacterSocket>(character))
                            continue;

                        var tpView = ecb.Instantiate(viewPrefabs.ThirdPersonViewPrefab);
                        
                        // Tag
                        ecb.AddComponent<ThirdPersonViewTag>(tpView);

                        // Transform
                        socketEntity = SystemAPI.GetComponent<ThirdPersonCharacterSocket>(character).Entity;
                        ecb.AddComponent(tpView, new Parent { Value = socketEntity });
                        ecb.SetComponent(tpView, LocalTransform.Identity);

                        // Data
                        ecb.AddComponent(tpView, new AttachedToCharacter { Entity = character });
                        ecb.AddComponent(tpView, new ItemViewOwner { Entity = itemEntity });
                        ecb.SetComponent(itemEntity, new LastViewEntity { Entity = tpView });
                    }
                    break;
                
                case ItemState.Inventory:
                    // just destroy view entity above
                    break;
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[BurstCompile]
public partial struct DestoryItemEntityViewWithoutOwnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (owner, viewEntity) in SystemAPI
            .Query<ItemViewOwner>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(owner.Entity))
            {
                ecb.DestroyEntity(viewEntity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}