using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.CharacterController;
using Unity.NetCode;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class FirstPersonPlayerInputsSystem : SystemBase
{
    private InputSystem_Actions inputActions;

    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
        inputActions = new InputSystem_Actions();
    }

    protected override void OnStartRunning()
    {
        inputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        inputActions.Disable();
    }

    
    protected override void OnUpdate()
    {
        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<FirstPersonPlayerInputs>, FirstPersonPlayer>().WithAll<GhostOwnerIsLocal>())
        {
            // Freeze character
            var menuState = SystemAPI.GetSingleton<MenuStateComponent>();
            if (menuState.State != MenuState.Game)
            {
                playerInputs.ValueRW.MoveInput = float2.zero;
                InputDeltaUtilities.AddInputDelta(ref playerInputs.ValueRW.LookInput, Vector2.zero);

                return;
            }

            playerInputs.ValueRW.MoveInput = (float2) inputActions.Player.Move.ReadValue<Vector2>();

            var lookDelta = inputActions.Player.Look.ReadValue<Vector2>() * player.LookInputSensitivity;
            InputDeltaUtilities.AddInputDelta(ref playerInputs.ValueRW.LookInput, lookDelta);

            playerInputs.ValueRW.JumpPressed = default;
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                playerInputs.ValueRW.JumpPressed.Set();
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(PredictedFixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct FirstPersonPlayerVariableStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, playerNetworkInput, player) in SystemAPI.Query<FirstPersonPlayerInputs, RefRW<FirstPersonPlayerNetworkInput>, FirstPersonPlayer>().WithAll<Simulate>())
        {
            // Compute input deltas, compared to last known values
            float2 lookInputDelta = InputDeltaUtilities.GetInputDelta(
                playerInputs.LookInput, 
                playerNetworkInput.ValueRO.LastProcessedLookInput);
            playerNetworkInput.ValueRW.LastProcessedLookInput = playerInputs.LookInput;

            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);
                characterControl.LookDegreesDelta = lookInputDelta;
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct FirstPersonPlayerFixedStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player) in SystemAPI.Query<FirstPersonPlayerInputs, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);
                
                quaternion characterRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation;

                // Move
                float3 characterForward = MathUtilities.GetForwardFromRotation(characterRotation);
                float3 characterRight = MathUtilities.GetRightFromRotation(characterRotation);
                characterControl.MoveVector = (playerInputs.MoveInput.y * characterForward) + (playerInputs.MoveInput.x * characterRight);
                characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                // Jump
                characterControl.Jump = playerInputs.JumpPressed.IsSet;
            
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }

    public static class AdditionalHelper
    {
        public static void FreezeCharacter()
        {

        }
    }      
}
