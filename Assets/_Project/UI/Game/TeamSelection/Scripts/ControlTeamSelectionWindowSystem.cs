// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;

// public partial class ControlTeamSelectionWindowSystem : SystemBase
// {
//     private EntityQuery m_LocalPlayerQuery;

//     protected override void OnCreate()
//     {
//         RequireForUpdate<TeamSelectionController>();

//         m_LocalPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
//             .WithAll<FirstPersonPlayer, GhostOwnerIsLocal, BelongsToMatchId, CurrentPlayerState>()
//             .Build(EntityManager);

//         RequireForUpdate(m_LocalPlayerQuery);
//     }
    
//     protected override void OnUpdate()
//     {
//         TeamSelectionController teamSelection = SystemAPI.ManagedAPI.GetSingleton<TeamSelectionController>();

//         CurrentPlayerState localPlayerState = m_LocalPlayerQuery.ToComponentDataArray<CurrentPlayerState>(Allocator.Temp)[0];

//         if (localPlayerState.Value == PlayerState.TeamSelection)
//         {
//             teamSelection.DisplayWindow();
//         }
//         else
//         {
//             teamSelection.HideWindow();
//         }
//     }
// }