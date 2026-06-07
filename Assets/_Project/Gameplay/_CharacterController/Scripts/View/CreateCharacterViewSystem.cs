using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// Automatically creates request for entity if it doesn't have tag
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[BurstCompile]
public partial struct DetectCharactersWithoutViewSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (_, entity) in SystemAPI
            .Query<FirstPersonCharacterComponent>()
            .WithNone<HasPlayerCharacterViewTag>()
            .WithEntityAccess())
        {

            var createViewRequest = ecb.CreateEntity();
            ecb.AddComponent(createViewRequest, new CreatePlayerCharacterViewRequest 
            { 
                Entity = entity 
            });
            
            UnityEngine.Debug.Log($"[DetectCharactersWithoutViewSystem] Create request");
        }

        ecb.Playback(state.EntityManager);
    }
}

/// <summary>
/// Receives and proccess request. Creates view entity and binds it
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// [BurstCompile]
public partial struct CreateCharacterViewSystem : ISystem
{
    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI
            .Query<CreatePlayerCharacterViewRequest>()
            .WithEntityAccess())
        {
            if (SystemAPI.HasComponent<LastPlayerCharacterView>(request.Entity))
            {
                var lastView = SystemAPI.GetComponent<LastPlayerCharacterView>(request.Entity);
                // Delete old view if it has
                if (SystemAPI.Exists(lastView.Entity))
                {
                    ecb.AddComponent<DestroyEntityTag>(lastView.Entity);
                }
            }

            UnityEngine.Debug.Log($"[CreateCharacterViewSystem] First check passed");


            if (!SystemAPI.HasComponent<PlayerCharacterViews>(request.Entity))
            {
                ecb.DestroyEntity(entity);    
                continue;
            }
            // Getting views
            var views = SystemAPI.GetComponent<PlayerCharacterViews>(request.Entity);
            
            UnityEngine.Debug.Log($"[CreateCharacterViewSystem] Second check passed");

            // For third-person
            if (!SystemAPI.HasComponent<LocalCharacterTag>(request.Entity))
            {
                // Create and bind view entity
                var TPViewEntity = ecb.Instantiate(views.TPView);
                ecb.AddComponent<ThirdPersonPlayerCharacterTag>(TPViewEntity);
                ecb.AddComponent(TPViewEntity, new PlayerCharacterViewOwner 
                { 
                    Entity = request.Entity 
                });
                
                // Setting ghost character data entity
                ecb.AddComponent<HasPlayerCharacterViewTag>(request.Entity);
                ecb.SetComponent(request.Entity, new LastPlayerCharacterView 
                { 
                    Entity = TPViewEntity 
                });
                ecb.AppendToBuffer(request.Entity, new LinkedEntityGroup 
                { 
                    Value = TPViewEntity 
                });

                // Setting Transform for view entity
                ecb.AddComponent(TPViewEntity, new Parent 
                { 
                    Value = request.Entity 
                });
                ecb.SetComponent(TPViewEntity, LocalTransform.Identity);

                UnityEngine.Debug.Log($"[CreateCharacterViewSystem] Create entity for entity {request.Entity}");
            }
            else
            {
                // TODO: Add First Person View 
                ecb.AddComponent<HasPlayerCharacterViewTag>(request.Entity);
                UnityEngine.Debug.Log($"[CreateCharacterViewSystem] Not create for entity {request.Entity}");
            }
            
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}