
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
    protected override void OnCreate()
    {
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        WorldSpaceUIController controller = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();

        foreach (var (target, cashed, entity) in SystemAPI
            .Query<RefRO<WorldUITargetEntity>, RefRW<CashedWorldUITargetEntityInfo>>()
            .WithEntityAccess())
        {
            var character = target.ValueRO.Entity;

            if (!SystemAPI.Exists(character)) 
                continue;
            
            if (!SystemAPI.HasComponent<CharacterName>(character) || 
                !SystemAPI.HasComponent<CurrentHealth>(character))
            {
                continue;
            }

            var playerName = SystemAPI.GetComponent<CharacterName>(character).Value;
            var playerHealth = SystemAPI.GetComponent<CurrentHealth>(character).Value;

            if (cashed.ValueRW.Name != playerName)
            {
                controller.SetName(character, playerName.ToString());
            }

            if (cashed.ValueRW.FillLength != Length.Percent(playerHealth))
            {
                controller.SetHealth(character, playerHealth);   
            }
        }
    }
}