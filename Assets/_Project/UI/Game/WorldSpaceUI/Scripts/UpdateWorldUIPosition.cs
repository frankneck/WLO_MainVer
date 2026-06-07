using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateWorldUIPositionSystem : SystemBase
{
    private Camera _camera;

    protected override void OnCreate()
    {
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        if (_camera == null)
            _camera = MainGameObjectCamera.Instance;

        if (_camera == null)
            return;

        float3 cameraPos = _camera.transform.position;

        var uiController = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();
        WorldSpaceControllerData controllerData = SystemAPI.GetSingleton<WorldSpaceControllerData>();

        foreach (var (target, offset, cached, entity) in SystemAPI
            .Query<RefRO<WorldUITargetEntity>, RefRO<WorldUIHeightOffset>, RefRW<CashedWorldUIData>>()
            .WithEntityAccess())
        {
            if (!SystemAPI.Exists(target.ValueRO.Entity))
                continue;

            if (!SystemAPI.ManagedAPI.HasComponent<GameObject>(entity))
                continue;

            if (!SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Entity))
                continue;

            var playerInfo = SystemAPI.ManagedAPI.GetComponent<GameObject>(entity);
            if (playerInfo == null)
                continue;

            float3 targetPos = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.Entity).Position;

            targetPos.y += offset.ValueRO.Value;

            // -------------------------
            // VISIBILITY (state change only)
            // -------------------------
            float distSq = math.distancesq(targetPos, cameraPos);
            bool isVisible = distSq <= controllerData.MaxDistance * controllerData.MaxDistance;

            bool wasVisible = cached.ValueRO.IsVisible;

            if (isVisible != wasVisible)
            {
                if (isVisible)
                    uiController.ShowWorldUIForEntity(target.ValueRO.Entity);
                else
                    uiController.HideWorldUIForEntity(target.ValueRO.Entity);

                cached.ValueRW.IsVisible = isVisible;
            }

            // -------------------------
            // POSITION UPDATE
            // -------------------------
            float3 prevPos = cached.ValueRO.Position;

            if (math.distancesq(prevPos, targetPos) > 0.0001f)
            {
                playerInfo.transform.position = targetPos;
                cached.ValueRW.Position = targetPos;
            }
        }
    }
}