using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct ProcessCreationMatchSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameModesPrefabs>();

        if (!SystemAPI.HasSingleton<MatchIdGeneratorComponent>())
        {
            Entity generatorId = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<MatchIdGeneratorComponent>(generatorId);
        }
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        ref var generator = ref SystemAPI.GetSingletonRW<MatchIdGeneratorComponent>().ValueRW;

        GameModesPrefabs gameModesPrefabs = SystemAPI.GetSingleton<GameModesPrefabs>();

        ProcessCreationMatchJob jobHandle = new ProcessCreationMatchJob
        {
            GeneratorId = generator,
            GameModesPrefabs = gameModesPrefabs,
            ECB = ecb 
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

// [BurstCompile]
public partial struct ProcessCreationMatchJob : IJobEntity
{
    public MatchIdGeneratorComponent GeneratorId;
    public GameModesPrefabs GameModesPrefabs;
    public EntityCommandBuffer ECB;

    public void Execute(
        CreateMatchWithUserSettings request,
        Entity requestEntity
    )
    {
        UnityEngine.Debug.Log("Create deathmatch entity");

        int choosedLevel = request.LevelMap;  
        GameMode choosedGameMode = request.GameMode;

        MatchId newId = new MatchId(GeneratorId.Value);

        switch (choosedGameMode)
        {
            case GameMode.Deathmatch :
                Entity deathmatchEntity = CreateAndSetDeathmatch(
                    ecb: ref ECB, 
                    matchId: newId, 
                    roundTime: request.DeathmatchRoundTime, 
                    maxPlayers: request.MaxPlayers,
                    numberOfRounds: request.DeathmatchNumberOfRounds
                );
                SendLoadLevelRequest(ref ECB, choosedLevel);

                break;
            
            case GameMode.Domination :
                Entity deathraceEntity = CreateAndSetDomination(
                    ecb: ref ECB, 
                    matchId: newId, 
                    matchTime: request.DominationMatchTime, 
                    rivavalTime: request.DominationRivavalTime,
                    maxScore: request.DominationMaxScore, 
                    maxPlayers: request.MaxPlayers
                );
                SendLoadLevelRequest(ref ECB, choosedLevel);

                break;

            default :
                break;
        }

        // Increment value 
        GeneratorId.Value++;

        ECB.DestroyEntity(requestEntity);
    }

    private Entity CreateAndSetDomination(
        ref EntityCommandBuffer ecb,
        MatchId matchId,
        float matchTime,
        float rivavalTime,
        int maxScore,
        int maxPlayers)
    {
        Entity matchEntity = ecb.Instantiate(GameModesPrefabs.Deathrace);

        ecb.SetComponent(matchEntity, new DominationMatchSettings
        {
            MaxPlayers = maxPlayers,
            MaxScore = maxScore,
            MatchTime = matchTime,
            RevivalTime = rivavalTime
        });

        ecb.SetComponent(matchEntity, new MatchIdComponent
        {
            Value = matchId
        });

        ecb.AddComponent<StartingMatchTag>(matchEntity);

        return matchEntity;
    }
    

    private Entity CreateAndSetDeathmatch(
        ref EntityCommandBuffer ecb,
        MatchId matchId,
        float roundTime,
        int numberOfRounds,
        int maxPlayers)
    {
        Entity matchEntity = ecb.Instantiate(GameModesPrefabs.Deathmatch);
        
        // max players number per one team
        var maxPlayersNumberPerTeam = maxPlayers / 2;

        ecb.SetComponent(matchEntity, new DeathmatchMatchSettings
        {
            RoundsNumber = numberOfRounds,
            RoundTime = roundTime,
            MaxPlayersNumberPerTeam = maxPlayersNumberPerTeam
        });

        ecb.SetComponent(matchEntity, new MatchIdComponent
        {
            Value = matchId
        });

        ecb.AddComponent<StartingMatchTag>(matchEntity);

        return matchEntity;
    }

    private void SendLoadLevelRequest(
        ref EntityCommandBuffer ecb,
        int levelIndex)
    {
        var loadLevelRequest = ecb.CreateEntity();
        ecb.AddComponent(loadLevelRequest, new LoadLevelRequest 
        { 
            LevelNumber = levelIndex 
        });
    }
}