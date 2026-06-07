
using System;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Invoked action event to destory GameObject and remove apropriated Entity 
/// </summary>
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class RemoveWorldUISystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var uiController = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();

        foreach( var (currentHealth, entity) in SystemAPI
            .Query<RefRO<CurrentHealth>>()
            .WithAll<EntityWithWorldUITag>()
            .WithEntityAccess())
        {
            if (currentHealth.ValueRO.Value < 0 )
            {
                uiController.RemoveWorldUIForEntity(entity);
                ecb.RemoveComponent<EntityWithWorldUITag>(entity);
            }
        }

        foreach (var (target, entity) in SystemAPI
            .Query<WorldUITargetEntity>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(target.Entity))
            {
                uiController.RemoveWorldUIForEntity(target.Entity);
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}


