using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;

[BurstCompile]
public partial struct AppllySlowingDownSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (slow, baseStats, character) in SystemAPI
            .Query<RefRO<SlowingDown>, RefRO<CharacterComponentBaseValues>, RefRW<FirstPersonCharacterComponent>>())
        {
            character.ValueRW.GroundMaxSpeed = baseStats.ValueRO.GroundMoveSpeed * slow.ValueRO.SpeedMultiplier;

            // НЕ слишком маленький Sharpness, иначе скользит
            character.ValueRW.GroundedMovementSharpness =
                baseStats.ValueRO.GroundedMovementSharpness * slow.ValueRO.SharpnessMultiplier;

            character.ValueRW.AirAcceleration = baseStats.ValueRO.AirAcceleration * slow.ValueRO.AirAccelerationMultiplier;
            character.ValueRW.AirMaxSpeed = baseStats.ValueRO.AirMaxSpeed * slow.ValueRO.AirMaxSpeedMultiplier;

            // AirDrag увеличиваем, чтобы убрать скольжение
            character.ValueRW.AirDrag = baseStats.ValueRO.AirDrag * slow.ValueRO.AirDragMultiplier;

            // Gravity чуть меньше, чтобы замедлить падение, но не “выталкивало”
            character.ValueRW.Gravity = baseStats.ValueRO.Gravity * slow.ValueRO.GravityMultiplier;

            character.ValueRW.JumpSpeed = baseStats.ValueRO.JumpSpeed * slow.ValueRO.JumpMultiplier;
        }   

        // normal values
        foreach (var (baseStats, character) in SystemAPI
            .Query<RefRO<CharacterComponentBaseValues>, RefRW<FirstPersonCharacterComponent>>()
            .WithNone<SlowingDown>())
        {
            character.ValueRW.GroundMaxSpeed = baseStats.ValueRO.GroundMoveSpeed;
            character.ValueRW.GroundedMovementSharpness = baseStats.ValueRO.GroundedMovementSharpness;

            character.ValueRW.AirAcceleration = baseStats.ValueRO.AirAcceleration;
            character.ValueRW.AirMaxSpeed = baseStats.ValueRO.AirMaxSpeed;

            character.ValueRW.AirDrag = baseStats.ValueRO.AirDrag;
            character.ValueRW.Gravity = baseStats.ValueRO.Gravity;
            character.ValueRW.JumpSpeed = baseStats.ValueRO.JumpSpeed;
        }
    }
}