using Unity.Burst;
using Unity.Entities;
using System.Collections.Generic;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct UpdatePlayerPingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //Создать словарь short NetworkID и short ping
        Dictionary<ushort, ushort> playerPingDictionary = new Dictionary<ushort, ushort>();

        //Получить все Entity с NetworkSnapshotAck
        foreach (var (nsa, nID) in SystemAPI.Query<RefRO<NetworkSnapshotAck>, RefRO<NetworkId>>())
        {
            //Получить NetworkID и ping
            ushort networkID = (ushort)nID.ValueRO.Value;
            ushort ping = (ushort)nsa.ValueRO.EstimatedRTT;

            //Добавить в словарь
            playerPingDictionary[networkID] = ping;
        }

        //Получить все Entity с компонентами PlayerPing и GhostOwner
        foreach (var (ping, owner) in SystemAPI.Query<RefRW<PlayerPing>, RefRO<GhostOwner>>())
        {
            ping.ValueRW.Value = playerPingDictionary[(ushort)owner.ValueRO.NetworkId];
        }
    }
}
