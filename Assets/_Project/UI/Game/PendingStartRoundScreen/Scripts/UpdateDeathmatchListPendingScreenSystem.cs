using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdatePendingStartMatchScreenSystem : SystemBase
{
    private EntityQuery m_PlayerQuery;
    private EntityQuery m_LocalPlayerQuery;

    private NativeHashSet<Entity> m_CurrentPlayers;
    private NativeHashSet<Entity> m_PreviousPlayers;

    protected override void OnCreate()
    {
        m_PlayerQuery = EntityManager.CreateEntityQuery(
            ComponentType.ReadOnly<CharacterName>(),
            ComponentType.ReadOnly<GameTeam>(),
            ComponentType.ReadOnly<BelongsToMatchId>(),
            ComponentType.ReadOnly<PlayerPing>(),
            ComponentType.ReadOnly<CurrentPlayerState>()
        );

        m_LocalPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<CurrentPlayerState, GhostOwnerIsLocal, BelongsToMatchId>()
            .Build(EntityManager);

        RequireForUpdate(m_PlayerQuery);

        m_CurrentPlayers = new NativeHashSet<Entity>(128, Allocator.Persistent);
        m_PreviousPlayers = new NativeHashSet<Entity>(128, Allocator.Persistent);

        RequireForUpdate<PendingStartMatchScreen>();
    }

    protected override void OnDestroy()
    {
        m_CurrentPlayers.Dispose();
        m_PreviousPlayers.Dispose();
    }

    protected override void OnUpdate()
    {
        var screenView = SystemAPI.ManagedAPI.GetSingleton<PendingStartMatchScreen>();

        var localMatchId = m_LocalPlayerQuery.ToComponentDataArray<BelongsToMatchId>(Allocator.Temp)[0];

        if (m_LocalPlayerQuery.IsEmpty)
            return;

        m_CurrentPlayers.Clear();

        var entities = m_PlayerQuery.ToEntityArray(Allocator.Temp);

        foreach (var e in entities)
        {
            var matchId = EntityManager.GetComponentData<BelongsToMatchId>(e);
            var playerState = EntityManager.GetComponentData<CurrentPlayerState>(e);

            // Different match -> skip
            if (matchId.MatchId != localMatchId.MatchId)
                continue;
            
            // Entity is not ready -> skip
            if (playerState.Value != PlayerState.PendingStartMatch)
                continue;
            
            // if entity is pendings -> add to currentPlayers 
            m_CurrentPlayers.Add(e);
        }

        entities.Dispose();

        bool changed = false;

        if (m_CurrentPlayers.Count != m_PreviousPlayers.Count)
        {
            changed = true;
        }
        else
        {
            foreach (var e in m_CurrentPlayers)
            {
                // if although one is different -> rebuild 
                if (!m_PreviousPlayers.Contains(e))
                {
                    changed = true;
                    break;
                }
            }
        }

        // if not changed -> skip
        if (!changed)
            return;

        m_PreviousPlayers.Clear();

        // Rewrite cashe
        foreach (var e in m_CurrentPlayers)
        {
            m_PreviousPlayers.Add(e);
        }

        var redList = new List<PendingStartMatchScreen.PlayerListData>();
        var blueList = new List<PendingStartMatchScreen.PlayerListData>();
        var playerList = new List<PendingStartMatchScreen.PlayerListData>();

        // Rebuild list
        foreach (var e in m_CurrentPlayers)
        {
            var name = EntityManager.GetComponentData<CharacterName>(e);
            var team = EntityManager.GetComponentData<GameTeam>(e);
            var ping = EntityManager.GetComponentData<PlayerPing>(e);

            var data = new PendingStartMatchScreen.PlayerListData(
                name.Value.ToString(),
                ping.Value
            );

            if (team.Value == TeamType.Red)
            {
                redList.Add(data);
            }
            else if (team.Value == TeamType.Blue)
            {
                blueList.Add(data);
            }
            else
            {
                playerList.Add(data);
            }
        }

        screenView.UpdatePlayerListDataForDeathmatch(redList, blueList);
        screenView.UpdatePlayerListDataForDomination(playerList);
    }
}