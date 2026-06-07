using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Confirms that the player character has been spawned and hides loading screen
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ConfirmPlayerSpawnOnClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ( var (_, receive, entity) in SystemAPI
            .Query<PlayerCharacterSpawned, ReceiveRpcCommandRequest>()
            .WithEntityAccess()) 
        {
            LoadingScreenUI.Set(LoadingScreenState.None);   
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}