using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct SummarizeRoundSystem : ISystem
{    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameMatchGlobalSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var globalSettings = SystemAPI.GetSingleton<GameMatchGlobalSettings>();

        foreach (var (request, requestEntity) in SystemAPI
            .Query<SummarizeRoundRequest>()
            .WithEntityAccess())
        {
            Entity matchEntity = request.MatchEntity;
            Entity roundEntity = request.RoundEntity;

            if (!SystemAPI.HasComponent<DeathmatchMatchSettings>(matchEntity) ||
                !SystemAPI.HasComponent<DeathmatchTeamsData>(matchEntity))
            {
                UnityEngine.Debug.Log($"[Rounds: SummarizeRoundSystem] Current entities from request isn't valid.");
                ecb.DestroyEntity(requestEntity);
                return;
            }

            var matchSettings = SystemAPI.GetComponent<DeathmatchMatchSettings>(matchEntity);
            var playedRoundsNumber = SystemAPI.GetComponentRW<PlayedRoundsNumber>(matchEntity);
            var teams = SystemAPI.GetComponentRW<DeathmatchTeamsData>(matchEntity);

            TeamType winnerTeam = DefineWinnerTeam(teams.ValueRW);

            int minPlayersNumberToStartMatch = globalSettings.MinPlayersPerTeamToStartDeathmatch;

            if (teams.ValueRW.BluePlayers < minPlayersNumberToStartMatch || 
                teams.ValueRW.RedPlayers < minPlayersNumberToStartMatch)
            {
                // Finish on all rounds are done
                // UnityEngine.Debug.Log($"[Rounds: SummarizeRoundSystem] Match is finished because min players to start from one of teams equals 0.");
                
                FinishCurrentMatch(
                    ref state, 
                    ref ecb, 
                    matchEntity
                );

                MessageClientDeathmatchIsFinished(
                    ref ecb, 
                    winnerTeam, 
                    teams.ValueRW.BluePlayersWins,
                    teams.ValueRW.RedPlayersWins
                );
            }
            else if (playedRoundsNumber.ValueRW.Value >= matchSettings.RoundsNumber)
            {
                // Finish on all rounds are done
                // UnityEngine.Debug.Log($"[Rounds: SummarizeRoundSystem] Match is finished because remaining number of rounds equals 0.");
                
                FinishCurrentMatch(
                    ref state, 
                    ref ecb, 
                    matchEntity
                );
                
                MessageClientDeathmatchIsFinished(
                    ref ecb, 
                    winnerTeam, 
                    teams.ValueRW.BluePlayersWins,
                    teams.ValueRW.RedPlayersWins
                );
            }

            SendClearRoundDataRequest(ref ecb, roundEntity);
            ecb.RemoveComponent<StartedRoundTag>(matchEntity);
        
            ecb.DestroyEntity(requestEntity);
        }   

        ecb.Playback(state.EntityManager);
    }

    private void MessageClientDeathmatchIsFinished(
        ref EntityCommandBuffer ecb,
        TeamType winnerTeam,
        int blueTeamScore,
        int redTeamScore
    )
    {
        var rpcEntity = ecb.CreateEntity();

        ecb.AddComponent(rpcEntity, new MessageClientsDeathmatchIsFinished
        {
            WinnerTeam = winnerTeam,
            BlueTeamScore = blueTeamScore,
            RedTeamScore = redTeamScore
        });

        ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
    }

    private TeamType DefineWinnerTeam(DeathmatchTeamsData teams)
    {
        TeamType winnerTeam;

        if (teams.RedPlayersWins > teams.BluePlayersWins)
        {
            winnerTeam = TeamType.Red;
        } 
        else if (teams.BluePlayersWins > teams.RedPlayersWins)
        {
            winnerTeam = TeamType.Blue;
        }
        else
        {
            winnerTeam = TeamType.None;
        }

        return winnerTeam;
    }

    private void FinishCurrentMatch(
        ref SystemState state,
        ref EntityCommandBuffer ecb,
        Entity matchEntity
    )
    {
        // Change player state
        ChangeMatchPlayersStateAndRemoveAssingedTag(
            ref state, 
            ref ecb, 
            matchEntity, 
            PlayerState.FinishingMatch
        );
        
        ecb.AddComponent<DefineFinishMatchTick>(matchEntity);
        ecb.AddComponent<FinishingMatchTag>(matchEntity);
        ecb.RemoveComponent<ActiveMatchTag>(matchEntity);
    }

    private void ChangeMatchPlayersStateAndRemoveAssingedTag(
        ref SystemState state,
        ref EntityCommandBuffer ecb,
        Entity matchEntity,
        PlayerState newPlayerState)
    {
        foreach (var (belongsToMatch, playerState, playerEntity) in SystemAPI
            .Query<BelongsToMatch, CurrentPlayerState>()
            .WithEntityAccess())
        {
            // Other match
            if (belongsToMatch.Entity != matchEntity)
                continue;

            PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(
                ref ecb, 
                playerEntity, 
                newPlayerState
            );

            ecb.RemoveComponent<CharacterAssignedTag>(playerEntity);
        }
    }

    private void SendClearRoundDataRequest(
        ref EntityCommandBuffer ecb,
        Entity roundEntity)
    {
        var request = ecb.CreateEntity();
        ecb.AddComponent(request, new ClearRoundData
        {
            RoundEntity = roundEntity
        });
    }
} 

public struct ClearRoundData : IComponentData
{
    public Entity RoundEntity;
}

public struct ClearMatchData : IComponentData
{
    public Entity MatchEntity;
}

public struct MessageClientsDeathmatchIsFinished : IRpcCommand
{
    public TeamType WinnerTeam;
    public int BlueTeamScore;
    public int RedTeamScore;
}