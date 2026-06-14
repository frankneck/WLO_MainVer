using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// Invokes action event to instantiate GameObject of WorldSpace UI Healthbar. 
/// </summary>
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class SpawnWorldUISystem : SystemBase
{
    private EntityQuery m_LocalPlayerCharacterGameTeamQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<WorldSpaceUIController>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var uiController = SystemAPI.ManagedAPI.GetSingleton<WorldSpaceUIController>();

        foreach (var (transform, playerTeam, entity) in SystemAPI
            .Query<LocalTransform, GameTeam>()
            .WithAll<CharacterTag, CurrentHealth, CharacterName>()
            .WithNone<LocalCharacterTag, EntityWithWorldUITag>()
            .WithEntityAccess())
        {
            uiController.SpawnAndSetHealthbarForEntity(ecb, transform.Position, entity);
            ecb.AddComponent<EntityWithWorldUITag>(entity);
        }
        
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

public enum TeamRelationship : byte
{
    None = 0,
    Friend,
    Enemy
}