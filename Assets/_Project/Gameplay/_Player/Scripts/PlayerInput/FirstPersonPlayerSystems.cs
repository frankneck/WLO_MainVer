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
    private InputAction[] slotActions;

    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerCommands>().Build());
        
        inputActions = new InputSystem_Actions();
        slotActions = new[]
        {
            inputActions.Player.SelectSlot1,
            inputActions.Player.SelectSlot2,
            inputActions.Player.SelectSlot3,
            inputActions.Player.SelectSlot4,
            inputActions.Player.SelectSlot5,
            inputActions.Player.SelectSlot6,
            inputActions.Player.SelectSlot7,
            inputActions.Player.SelectSlot8,
            inputActions.Player.SelectSlot9,
        };
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
        foreach (var (playerCommands, inputPermissions, player) in SystemAPI
            .Query<RefRW<FirstPersonPlayerCommands>, RefRO<InputPermissions>, FirstPersonPlayer>()
            .WithAll<GhostOwnerIsLocal>())
        {
            // TODO : Use input permisions instead using UI Controller in this system!
            if (UIController.Instance.HasStateFlag(GameUIWindowsState.Inventory) || 
                UIController.Instance.HasStateFlag(GameUIWindowsState.GameMenu))
            {
                inputActions.Disable();
            }
            else
            {
                inputActions.Enable();
            }

            // Freeze moving
            if (inputPermissions.ValueRO.Value.HasFlag(InputFlags.Move))
                inputActions.Player.Move.Enable();
            else
                inputActions.Player.Move.Disable();

            // Freeze looking
            if (inputPermissions.ValueRO.Value.HasFlag(InputFlags.Look))
                inputActions.Player.Look.Enable();
            else
                inputActions.Player.Look.Disable();

            // Move
            playerCommands.ValueRW.MoveInput = (float2) inputActions.Player.Move.ReadValue<Vector2>();
            var lookDelta = inputActions.Player.Look.ReadValue<Vector2>() * player.LookInputSensitivity;
            InputDeltaUtilities.AddInputDelta(ref playerCommands.ValueRW.LookInput, lookDelta);

            // Jump
            playerCommands.ValueRW.JumpPressed = default;
            if (inputActions.Player.Jump.IsPressed())
            {
                playerCommands.ValueRW.JumpPressed.Set();
            }

            // Freeze shooting
            if (inputPermissions.ValueRO.Value.HasFlag(InputFlags.Shoot))
                inputActions.Player.Shoot.Enable();
            else
                inputActions.Player.Shoot.Disable();

            // Use main action
            playerCommands.ValueRW.MainActionPressed = default;
            if (inputActions.Player.Shoot.IsPressed())
            {
                playerCommands.ValueRW.MainActionPressed.Set();
            }

            // Use shield
            playerCommands.ValueRW.ShieldHeld = default;
            if (inputActions.Player.Shield.IsPressed())
            {
                playerCommands.ValueRW.ShieldHeld = true;
            }

            // Selection equipment
            playerCommands.ValueRW.WeaponDirectIndex = -1;
            playerCommands.ValueRW.HasDirectWeaponSelect = false;

            for (int i = 0; i < slotActions.Length; i++)
            {
                if (slotActions[i].IsPressed())
                {
                    playerCommands.ValueRW.HasDirectWeaponSelect = true;
                    playerCommands.ValueRW.WeaponDirectIndex = i;
                }
            }

            // Scroll 
            float scrollY = inputActions.Player.WeaponScroll.ReadValue<Vector2>().y;

            int delta = 0;

            if (scrollY > 0.1f)
            {
                delta = -1;
            }
            else if (scrollY < -0.1f) 
            {
                delta = 1;
            } 

            playerCommands.ValueRW.WeaponScrollDelta = delta;

            // Previous slot
            if (inputActions.Player.PreviousWeapon.WasPressedThisFrame()) 
                playerCommands.ValueRW.PreviousWeaponPressed.Set();

            // Interact
            if (inputPermissions.ValueRO.Value.HasFlag(InputFlags.Interact))
                inputActions.Player.Interact.Enable();
            else
                inputActions.Player.Interact.Disable();

            playerCommands.ValueRW.InteractPressed = default;
            if (inputActions.Player.Interact.WasPressedThisFrame())
            {
                playerCommands.ValueRW.InteractPressed.Set();
            }
            
            // Interact
            if (inputPermissions.ValueRO.Value.HasFlag(InputFlags.Drop))
                inputActions.Player.Drop.Enable();
            else
                inputActions.Player.Drop.Disable();
            
            // Drop item
            playerCommands.ValueRW.DropPressed = default;
            if (inputActions.Player.Drop.WasPressedThisFrame())
            {
                playerCommands.ValueRW.DropPressed.Set();
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
        
        state.RequireForUpdate(SystemAPI
            .QueryBuilder()
            .WithAll<FirstPersonPlayer, FirstPersonPlayerCommands>()
            .Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        FirstPersonPlayerVariableStepControlJob job = new FirstPersonPlayerVariableStepControlJob
        {
            CharacterControlLookup = SystemAPI.GetComponentLookup<FirstPersonCharacterControl>(false),
            ActiveItemlLookup = SystemAPI.GetComponentLookup<ActiveItem>(true),
            ItemControlLookup = SystemAPI.GetComponentLookup<ItemControl>(false),
            CharacterActionControlLookup = SystemAPI.GetComponentLookup<CharacterActionControl>(false),
        };
        state.Dependency = job.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Simulate))]
public partial struct FirstPersonPlayerVariableStepControlJob : IJobEntity
{
    public ComponentLookup<FirstPersonCharacterControl> CharacterControlLookup;
    [ReadOnly] public ComponentLookup<ActiveItem> ActiveItemlLookup;
    public ComponentLookup<ItemControl> ItemControlLookup;
    public ComponentLookup<CharacterActionControl> CharacterActionControlLookup;

    void Execute(
        ref FirstPersonPlayerCommands playerCommands,
        ref FirstPersonPlayerNetworkInput playerNetworkCommands, 
        in FirstPersonPlayer player)
    {
        // Compute input deltas, compared to last known values
        float2 lookInputDelta = InputDeltaUtilities.GetInputDelta(
            playerCommands.LookInput, 
            playerNetworkCommands.LastProcessedLookInput);
        playerNetworkCommands.LastProcessedLookInput = playerCommands.LookInput;

        // Character
        if (CharacterControlLookup.HasComponent(player.ControlledCharacter))
        {
            FirstPersonCharacterControl characterControl = CharacterControlLookup[player.ControlledCharacter];
            
            // Look
            characterControl.LookDegreesDelta = lookInputDelta;
            
            CharacterControlLookup[player.ControlledCharacter] = characterControl;
        }
        
        // Active item
        if (ActiveItemlLookup.HasComponent(player.ControlledCharacter))
        {
            var activeItem = ActiveItemlLookup[player.ControlledCharacter];
            
            if (ItemControlLookup.HasComponent(activeItem.Entity))
            {
                ItemControl itemControl = ItemControlLookup[activeItem.Entity];

                itemControl.MainActionPressed = playerCommands.MainActionPressed.IsSet;
            
                ItemControlLookup[activeItem.Entity] = itemControl;
            }
        }

        // Character interact, drop
        if (CharacterActionControlLookup.HasComponent(player.ControlledCharacter))
        {
            CharacterActionControl characterActionControl = CharacterActionControlLookup[player.ControlledCharacter];
            
            characterActionControl.Interact = playerCommands.InteractPressed.IsSet;
            characterActionControl.Drop = playerCommands.DropPressed.IsSet;

            CharacterActionControlLookup[player.ControlledCharacter] = characterActionControl;
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
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerCommands>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerCommands, player) in SystemAPI.Query<FirstPersonPlayerCommands, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);
                
                quaternion characterRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation;

                // Move
                float3 characterForward = MathUtilities.GetForwardFromRotation(characterRotation);
                
                float3 characterRight = MathUtilities.GetRightFromRotation(characterRotation);
                
                characterControl.MoveVector = (playerCommands.MoveInput.y * characterForward) + (playerCommands.MoveInput.x * characterRight);
                
                characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                // Jump
                characterControl.Jump = playerCommands.JumpPressed.IsSet;
            
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }    
}
