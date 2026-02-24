using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct SpawnHealthBarSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthBarSpawner>();
        state.RequireForUpdate<CurrentHitPoints>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<HealthBarSpawner>());
        var spawner = query.GetSingleton<HealthBarSpawner>();

        foreach (var (_, entity) in SystemAPI
            .Query<RefRO<CurrentHitPoints>>()
            .WithEntityAccess().WithNone<HealthUI, GhostOwnerIsLocal>())
        {
            var go = Object.Instantiate(spawner.HealthBarPrefab);
            var image = go.GetComponentsInChildren<Image>();

            ecb.AddComponent(entity, new HealthUI
            {
               HealthBar = go.transform,
               HealthSlider = image[1],
               OpponentHeightOffset = spawner.OpponentHeightOffset,
               PlayerTowardCameraOffset = spawner.PlayerTowardCameraOffset,
               PlayerHeightOffset = spawner.PlayerHeightOffset
            });
        }
        ecb.Playback(state.EntityManager);
    }
} 