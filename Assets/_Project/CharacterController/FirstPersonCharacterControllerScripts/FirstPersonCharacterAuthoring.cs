using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Unity.Rendering;

[DisallowMultipleComponent]
public class FirstPersonCharacterAuthoring : MonoBehaviour
{
    public GameObject ViewEntity;
    public GameObject CharacterBody;
    public GameObject CharacterVisor;
    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();

    public float GroundMaxSpeed = 10f;
    public float GroundedMovementSharpness = 15f;
    public float AirAcceleration = 50f;
    public float AirMaxSpeed = 10f;
    public float AirDrag = 0f;
    public float JumpSpeed = 10f;
    public float3 Gravity = math.up() * -30f;
    public bool PreventAirAccelerationAgainstUngroundedHits = true;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault();
    public float MinViewAngle = -90f;
    public float MaxViewAngle = 90f;

    public class Baker : Baker<FirstPersonCharacterAuthoring>
    {
        public override void Bake(FirstPersonCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring.gameObject, authoring.CharacterProperties);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, new FirstPersonCharacterComponent
            {
                GroundMaxSpeed = authoring.GroundMaxSpeed,
                GroundedMovementSharpness = authoring.GroundedMovementSharpness,
                AirAcceleration = authoring.AirAcceleration,
                AirMaxSpeed = authoring.AirMaxSpeed,
                AirDrag = authoring.AirDrag,
                JumpSpeed = authoring.JumpSpeed,
                Gravity = authoring.Gravity,
                PreventAirAccelerationAgainstUngroundedHits = authoring.PreventAirAccelerationAgainstUngroundedHits,
                StepAndSlopeHandling = authoring.StepAndSlopeHandling,
                MinViewAngle = authoring.MinViewAngle,
                MaxViewAngle = authoring.MaxViewAngle,
                ViewEntity = GetEntity(authoring.ViewEntity, TransformUsageFlags.Dynamic),
                ViewPitchDegrees = 0f,
                ViewLocalRotation = quaternion.identity,
            });
            AddComponent(entity, new FirstPersonCharacterControl());
            AddComponent(entity, new NewPlayerTag());

            AddComponent(entity, new CharacterRender
            {
                CharacterBody  = GetEntity(authoring.CharacterBody,  TransformUsageFlags.Dynamic), 
                CharacterVisor = GetEntity(authoring.CharacterVisor, TransformUsageFlags.Dynamic)
            });
        }
    }
}
