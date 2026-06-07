using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ClientDeathmatchFinishSystem : SystemBase
{
    private EntityQuery m_PLayersQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<FinishingMatchScreen>();   

        m_PLayersQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<KDCounter, PlayerName, GameTeam>().Build(EntityManager);
    }

    protected override void OnUpdate()
    {
        var finishingScreen = SystemAPI.ManagedAPI.GetSingleton<FinishingMatchScreen>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var playerEntities = m_PLayersQuery.ToEntityArray(Allocator.Temp);

        foreach (var (rpc, receive, rpcEntity) in SystemAPI
            .Query<MessageClientsDeathmatchIsFinished, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            finishingScreen.UpdateListViewsData(EntityManager, playerEntities);
            finishingScreen.SetMainTitle(rpc.WinnerTeam, rpc.BlueTeamScore, rpc.RedTeamScore);

            ecb.DestroyEntity(rpcEntity);
        }
        
        playerEntities.Dispose();

        ecb.Playback(EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateNextMatchTimerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FinishingMatchScreen>();   
    }

    protected override void OnUpdate()
    {
        var finishingScreen = SystemAPI.ManagedAPI.GetSingleton<FinishingMatchScreen>();

        foreach (var (match, _) in SystemAPI
            .Query<BelongsToMatch, FirstPersonPlayer>()
            .WithAll<GhostOwnerIsLocal>())
        {
            if (!SystemAPI.HasComponent<LeftSecondsToFinishRoundTimer>(match.Entity))
                continue;

            UnityEngine.Debug.Log("UpdateNextMatchTimerSystem");

            var timeInSecondsToStartNewRound = SystemAPI.GetComponent<LeftSecondsToFinishRoundTimer>(match.Entity);
            finishingScreen.UpdateTimer(timeInSecondsToStartNewRound.Value);
        }
    }
}