using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[BurstCompile]
public partial struct ProcessClientTeamSelectionRequestSystem : ISystem
{
    private EntityQuery m_LocalPlayerQuery;

    public void OnCreate(ref SystemState state)
    {
        m_LocalPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<FirstPersonPlayer, GhostOwnerIsLocal>().Build(state.EntityManager);
        
        state.RequireForUpdate(m_LocalPlayerQuery);
        state.RequireForUpdate<NetworkStreamConnection>();
    } 

    public void OnUpdate(ref SystemState state)
    {
        Entity connectionEntity = SystemAPI.GetSingletonEntity<NetworkStreamConnection>();   

        // Only one Local player entity on the client
        Entity localPlayer = m_LocalPlayerQuery.ToEntityArray(Allocator.Temp)[0]; 

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI
            .Query<ClientJoinPlayerTeam>()
            .WithEntityAccess())
        {
            var rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new TryJoinPlayerToTeamRpc
            {
                PlayerEntity = localPlayer,
                PlayerTeam = request.RequestedTeamType
            });

            ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
            {
                TargetConnection = connectionEntity
            });

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct TryJoinPlayerToTeamRpc : IRpcCommand
{
    public Entity PlayerEntity;
    public TeamType PlayerTeam;
}