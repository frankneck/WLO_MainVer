using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UpdateDeathmatchHudWinnerWindowSystems : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
        var hudView = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();

        var ecb = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);

        foreach (var (rpc, receive, rpcEntity) in SystemAPI
            .Query<UpdateRoundTeamWinnerRpc, ReceiveRpcCommandRequest>()
            .WithEntityAccess())
        {
            UnityEngine.Debug.Log($"[UpdateDeathmatchHudWinnerSystem] Rpc received on client. Current winner is {rpc.Value}");

            hudView.SetWinnerContainer(rpc.Value);

            ecb.DestroyEntity(rpcEntity);
        }
    }
}

/// <summary>
/// It used to update winner container for rounding match (deathmatch)
/// </summary>
public struct UpdateRoundTeamWinnerRpc : IRpcCommand
{
    public TeamType Value;
}

/// <summary>
/// It used to update winner container for match without rounds (domination)
/// </summary>
public struct UpdatePlayerWinnerRpc : IRpcCommand
{
    public CharacterName Value;
}

// Две системы
// 1-я - обновляет для раундов
// 2-я - обновляет для завершения матча

// Вариант с отправкой RPC 
// Принимаем RPC
// Обновляем hud