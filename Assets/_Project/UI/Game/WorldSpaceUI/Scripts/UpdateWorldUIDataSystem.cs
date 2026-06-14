
using Unity.Collections;
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
        RequireForUpdate<LocalCharacterTag>();
        RequireForUpdate<GameTeam>();
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Getting local player team
        var localPlayerCharacterEntity = SystemAPI.GetSingletonEntity<LocalCharacterTag>();

        if (!SystemAPI.HasComponent<GameTeam>(localPlayerCharacterEntity))
            return;

        GameTeam localPlayerTeam = SystemAPI.GetComponent<GameTeam>(localPlayerCharacterEntity);

        WorldSpaceUIController controller = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();

        foreach (var (target, cashed, worldUiEntity) in SystemAPI
            .Query<RefRO<WorldUITargetEntity>, RefRW<CashedWorldUITargetEntityInfo>>()
            .WithEntityAccess())
        {
            var characterEntity = target.ValueRO.Entity;

            if (!SystemAPI.Exists(characterEntity)) 
                continue;
            
            if (!SystemAPI.HasComponent<CharacterName>(characterEntity) || 
                !SystemAPI.HasComponent<CurrentHealth>(characterEntity))
            {
                continue;
            }

            var playerName = SystemAPI.GetComponent<CharacterName>(characterEntity).Value;
            var playerHealth = SystemAPI.GetComponent<CurrentHealth>(characterEntity).Value;

            if (cashed.ValueRW.Name != playerName)
            {
                controller.SetName(characterEntity, playerName.ToString());
            }

            if (cashed.ValueRW.FillLength != Length.Percent(playerHealth))
            {
                controller.SetHealth(characterEntity, playerHealth);   
            }

            if (!SystemAPI.HasComponent<RelationShipInitialized>(worldUiEntity))
            {
                var playerTeam = SystemAPI.GetComponent<GameTeam>(characterEntity);

                TeamRelationship relationToLocalPlayer = localPlayerTeam.Value == playerTeam.Value ?
                    TeamRelationship.Friend :
                    TeamRelationship.Enemy; 

                controller.SetHealthbarOnRelationship(ecb, characterEntity, relationToLocalPlayer, worldUiEntity);
            }
        }

        ecb.Playback(EntityManager);
    }
}

public struct RelationShipInitialized : IComponentData { }