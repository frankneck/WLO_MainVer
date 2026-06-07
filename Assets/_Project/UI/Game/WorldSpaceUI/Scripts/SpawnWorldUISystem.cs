using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Invokes action event to instantiate GameObject of WorldSpace UI Healthbar. 
/// </summary>
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class SpawnWorldUISystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var uiController = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();

        foreach (var (transform, entity) in SystemAPI
            .Query<LocalTransform>()
            .WithAll<CharacterTag, CurrentHealth, PlayerName>()
            .WithNone<GhostOwnerIsLocal, EntityWithWorldUITag>()
            .WithEntityAccess())
        {
            uiController.SpawnWorldUIForEntity(ecb, transform.Position, entity);
            ecb.AddComponent<EntityWithWorldUITag>(entity);
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}