using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ProcessClientDisconnectIntentionSystem : ISystem
{
    private EntityQuery m_LocalPlayerQuery;

    public void OnCreate(ref SystemState state)
    {
        m_LocalPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<CurrentPlayerState, GhostOwnerIsLocal, BelongsToMatchId>()
            .Build(state.EntityManager);

        state.RequireForUpdate(m_LocalPlayerQuery);
        state.RequireForUpdate<NetworkStreamConnection>();
    }
    public void OnUpdate(ref SystemState state)
    {
        Entity localPlayer = m_LocalPlayerQuery.ToEntityArray(Allocator.Temp)[0];

        var connection = SystemAPI.GetSingletonEntity<NetworkStreamConnection>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI
            .Query<ClientOnDisconnectButtonRequest>()
            .WithEntityAccess())
        {
            var rpc = ecb.CreateEntity(); 
            ecb.AddComponent(rpc, new ClientDisconnectRpc
            {
                Player = localPlayer
            });
            ecb.AddComponent(rpc, new SendRpcCommandRequest
            {
                TargetConnection = connection
            });

            UnityEngine.Debug.Log("Disconnect: first request from client");

            ecb.DestroyEntity(entity);
        }

       ecb.Playback(state.EntityManager);
    }
}