// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.NetCode;
// using UnityEngine.UIElements;

// [UpdateInGroup(typeof(PresentationSystemGroup))]
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct UIHudAddTeamPlayerInfoWindow : ISystem
// {
//     private GameTeam _localPlayerTeam;

//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<HudView>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);
//         var hudView = SystemAPI.ManagedAPI.GetSingleton<HudView>();


//         foreach (var team in SystemAPI
//             .Query<GameTeam>()
//             .WithAll<CharacterTag, GhostOwnerIsLocal>())
//         {
//             _localPlayerTeam = team;
//             break;
//         }

//         foreach (var (_, playerTeam, entity) in SystemAPI
//             .Query<CharacterTag, GameTeam>()
//             .WithNone<GhostOwnerIsLocal, TeamPlayerTag>()
//             .WithEntityAccess())
//         {
//             if (playerTeam.Value != _localPlayerTeam.Value) continue;

//             Entity windowEntity = ecb.CreateEntity();
//             VisualElement treeInstance = hudView.AddTeamWindow();
            
//             var container = treeInstance.Q<VisualElement>("team-player-info__container");
//             var healthFill = treeInstance.Q<VisualElement>("team-player-info__health");
//             var playerName = treeInstance.Q<Label>("team-player-info__name");

//             ecb.AddComponent(windowEntity, new TeamWindowUIData
//             {
//                 Container = container,
//                 HealthFill = healthFill,
//                 Name = playerName
//             });

//             ecb.AddComponent(windowEntity, new TeamWindowPlayer { Entity = entity });   
            
//             ecb.AddComponent<TeamPlayerTag>(entity);
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }


// [UpdateInGroup(typeof(PresentationSystemGroup))]
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct UIHudUpdateTeamPlayerInfoWindow : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<HudView>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (player, uiData, entity) in SystemAPI
//             .Query<TeamWindowPlayer, TeamWindowUIData>()
//             .WithEntityAccess())
//         {
//             var playerEntity = player.Entity;

//             if (!SystemAPI.HasComponent<CharacterName>(playerEntity) ||
//                 !SystemAPI.HasComponent<CurrentHealth>(playerEntity)) continue;

//             var health = SystemAPI.GetComponent<CurrentHealth>(playerEntity).Value;
//             var name = SystemAPI.GetComponent<CharacterName>(playerEntity).Value.ToString();

//             uiData.HealthFill.style.width = Length.Percent(health);
//             uiData.Name.text = name;
//         }
//     }
// }

// [UpdateInGroup(typeof(PresentationSystemGroup))]
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial struct UIHudRemoveTeamPlayerInfoWindow : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<HudView>();
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.Temp);

//         foreach (var (player, uiData, entity) in SystemAPI
//             .Query<TeamWindowPlayer, TeamWindowUIData>()
//             .WithEntityAccess())
//         {
//             if (!SystemAPI.Exists(player.Entity))
//             {
//                 uiData.Container.RemoveFromHierarchy();
//                 ecb.DestroyEntity(entity);
//             }
//         }

//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }

// public struct TeamWindowPlayer : IComponentData
// {
//     public Entity Entity;
// }

// public struct TeamPlayerTag : IComponentData { }

// public class TeamWindowUIData : IComponentData
// {
//     public VisualElement Container;
//     public VisualElement HealthFill;
//     public Label Name;
// }