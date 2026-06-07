#if !UNITY_SERVER
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Reads player input 
/// </summary>
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerInputUISystem : SystemBase
{
    private EntityQuery m_InputPermissionsQuery;

    protected override void OnCreate()
    {
        m_InputPermissionsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputPermissions, GhostOwnerIsLocal>()
            .Build(EntityManager);

        RequireForUpdate(m_InputPermissionsQuery);
    }

    protected override void OnUpdate()
    {        
        var InputPermissions = m_InputPermissionsQuery
            .ToComponentDataArray<InputPermissions>(Allocator.Temp)[0];

        var em = EntityManager;

        // App menu
        if (PlayerInput.AppMenu.WasPerformedThisFrame())
        {
            if (!InputPermissions.Value.HasFlag(InputFlags.Menu))
                return;

            UnityEngine.Debug.Log("[ECS] pressed");
            UIController.Instance.OnAppMenuPressed();
        }
        
        // Inventory
        if (PlayerInput.Inventory.WasPerformedThisFrame())
        {
            if (!InputPermissions.Value.HasFlag(InputFlags.Inventory))
                return;

            UnityEngine.Debug.Log("[I] pressed");
            UIController.Instance.OnInventoryPressed();
        }

        // Player list
        if (PlayerInput.PlayersList.WasPerformedThisFrame())
        {
            UnityEngine.Debug.Log("[TAB] pressed");
            UIController.Instance.OnPlayerListPressed();
        }
        else if (PlayerInput.PlayersList.WasReleasedThisFrame())
        {
            UnityEngine.Debug.Log("[TAB] released");
            UIController.Instance.OnPlayerListReleased();
        }
    }
}
#endif
