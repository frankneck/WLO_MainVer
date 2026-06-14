using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine.Serialization;

public struct PlayerTag : IComponentData { }

[Serializable]
[GhostComponent]
public struct FirstPersonPlayer : IComponentData
{
    [GhostField()] 
    public Entity ControlledCharacter;
    [FormerlySerializedAs("LookRotationSpeed")] public float LookInputSensitivity;
}

[Serializable]
public struct FirstPersonPlayerCommands : IInputComponentData
{
    // Movement
    
    public float2 MoveInput;
    public float2 LookInput;
    public InputEvent JumpPressed;

    // Shooting

    public InputEvent MainActionPressed;
    
    // Switch previous slot

    public InputEvent PreviousWeaponPressed;
    
    // Select slot
    public int WeaponDirectIndex;
    public bool HasDirectWeaponSelect;

    // Scroll

    public int WeaponScrollDelta;

    // SHIELD can be removed in future versions 
    
    public bool ShieldHeld;

    // Interact
    public InputEvent InteractPressed;
    public InputEvent DropPressed;
}

[Serializable]
[GhostComponent(SendTypeOptimization = GhostSendType.OnlyPredictedClients)]
public struct FirstPersonPlayerNetworkInput : IComponentData
{
    [GhostField()]
    public float2 LastProcessedLookInput;
}

/// <summary>
/// Marks what slot index is selected
/// </summary>
[GhostComponent()]
public struct SelectedSlotIndex : IInputComponentData
{
    [GhostField()] public int Value;
}