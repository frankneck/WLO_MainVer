using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateHealthBarSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<HealthUI>();
        RequireForUpdate<CurrentHitPoints>();
    }

    protected override void OnUpdate()
    {
        var m_mainCamera = Camera.main;

        foreach(var (healthUI, hp, maxHp, act, owner, transform, entity) in SystemAPI
            .Query<HealthUI, RefRO<CurrentHitPoints>, RefRO<MaxHitPoints>, RefRO<AutoCommandTarget>, RefRO<GhostOwner>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            if (!EntityManager.IsComponentEnabled<GhostOwnerIsLocal>(entity))
            {
                var targetHealthBarPos = transform.ValueRO.Position;
                targetHealthBarPos.y += healthUI.OpponentHeightOffset;
                var dirToCam = m_mainCamera.transform.position - (Vector3)targetHealthBarPos;
                dirToCam.y = 0;
                healthUI.HealthBar.SetPositionAndRotation(targetHealthBarPos, Quaternion.LookRotation(dirToCam));
            }

            var hpNormalized = math.saturate((float)hp.ValueRO.Value / maxHp.ValueRO.Value);
            var playerColor = NetworkIdDebugColorUtility.GetColor(owner.ValueRO.NetworkId);

            if (act.ValueRO.Enabled)
            {
                healthUI.HealthSlider.color = playerColor;
            }
            else
            {
                hpNormalized = 0;
                playerColor.a = 0.3f;
                healthUI.HealthSlider.transform.parent.GetComponent<Image>().color = playerColor;
            }

            healthUI.HealthSlider.fillAmount = hpNormalized;
        }
    }
}