using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;
using Unity.NetCode;

[Serializable]
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct FirstPersonCharacterComponent : IComponentData
{
    public float GroundMaxSpeed;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float JumpSpeed;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    public float MinViewAngle;
    public float MaxViewAngle;

    public bool MovementEnabled;

    [GhostField()] 
    public float ViewPitchDegrees;
    public quaternion ViewLocalRotation;
}

/// <summary>
/// Default character property valuse like Movespeed, Gravity etc.
/// Use this if you change current character properties.
/// 
/// <para> TODO: Add modificators instead this component </para>  
/// </summary>
[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct CharacterComponentBaseValues : IComponentData
{
    public float GroundMoveSpeed;
    public float3 Gravity;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float JumpSpeed;
}

[Serializable]
public struct FirstPersonCharacterControl : IComponentData
{
    public float3 MoveVector;
    public float2 LookDegreesDelta;
    public bool Jump;
}

[Serializable]
public struct FirstPersonCharacterView : IComponentData
{
    public Entity CharacterEntity;
}

/// <summary>
/// Need to store MainCamera
/// </summary>
[Serializable]
public struct FirstPersonCharacterViewReference : IComponentData
{
    public Entity ViewEntity;
}

[GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
public struct ActiveItem : IComponentData
{
    [GhostField] public Entity Entity;
}

public struct LastActiveItem : IComponentData
{
    public Entity Entity;
}

public struct SelectedWeaponRequest : IComponentData
{
    public Entity ChoosedStuff;
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct WeaponCastDelayTargetTicks : ICommandData
{
    public NetworkTick Tick { get ; set ; }
    public NetworkTick Value;
}

public struct OffsetForSpellSpawn : IComponentData
{
    public float3 Value;
}

// champion tag
public struct CharacterTag : IComponentData { }

public struct PlayerCharacterTag : IComponentData { } 

// Local champion
public struct LocalCharacterTag : IComponentData { }

/// <summary>
/// Inventory Actinos
/// </summary>
[Serializable]
public struct CharacterInteractionControl : IComponentData
{
    public bool Interact;
}