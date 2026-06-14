using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct AssignCharacterForPlayerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (characterToAssign, player, playerEntity) in SystemAPI
            .Query<AbleToAssignCharacter, RefRW<FirstPersonPlayer>>()
            .WithEntityAccess())
        {
            player.ValueRW.ControlledCharacter = characterToAssign.CharacterEntity;

            PlayerStateHelper.SendUpdateCurrentPlayerStateRequest(ref ecb, playerEntity, PlayerState.PendingStartRound);

            ecb.AddComponent<CharacterAssignedTag>(playerEntity);
    
            ecb.RemoveComponent<AbleToAssignCharacter>(playerEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}