
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Updates data of GameObject (name, healthbar length) and keep values in cashed component
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateWorldUIDataSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (target, cashed, entity) in SystemAPI
            .Query<RefRO<WorldUITargetEntity>, RefRW<CashedWorldUIData>>()
            .WithEntityAccess())
        {
            var character = target.ValueRO.Entity;

            if (!SystemAPI.Exists(character)) 
                continue;

            if (!SystemAPI.ManagedAPI.HasComponent<WorldUIElements>(entity)) 
                continue;
            
            if (!SystemAPI.HasComponent<PlayerName>(character) || 
                !SystemAPI.HasComponent<CurrentHealth>(character))
            {
                continue;
            }

            WorldUIElements uiElements = SystemAPI.ManagedAPI.GetComponent<WorldUIElements>(entity);

            var playerName = SystemAPI.GetComponent<PlayerName>(character).Value;
            var playerHealth = SystemAPI.GetComponent<CurrentHealth>(character).Value;

            if (cashed.ValueRW.Name != playerName)
            {
                uiElements.PlayerName.text = playerName.ToString();
                cashed.ValueRW.Name = playerName.ToString();
                Debug.Log("$[UpdateWorldUIDataSystem] Name updated");
            }

            if (cashed.ValueRW.FillLength != Length.Percent(playerHealth))
            {
                uiElements.HealthFill.style.width = Length.Percent(playerHealth);
                cashed.ValueRW.FillLength = Length.Percent(playerHealth);
                Debug.Log("$[UpdateWorldUIDataSystem] Healthbar updated");
            }
        }
    }
}