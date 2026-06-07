using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;

[DisallowMultipleComponent]
public class FirstPersonCharacterAuthoring : MonoBehaviour
{
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
                ViewPitchDegrees = 0f,
                ViewLocalRotation = quaternion.identity,
            });
            
            // This is default speed value
            AddComponent(entity, new CharacterComponentBaseValues { 
                GroundMoveSpeed = authoring.GroundMaxSpeed,
                AirAcceleration = authoring.AirAcceleration,
                AirDrag = authoring.AirDrag,
                AirMaxSpeed = authoring.AirMaxSpeed,
                JumpSpeed = authoring.JumpSpeed,
                Gravity = authoring.Gravity,
                GroundedMovementSharpness = authoring.GroundedMovementSharpness
            });

            AddComponent(entity, new FirstPersonCharacterControl());
            AddComponent<NetworkEntityReference>(entity);

            AddComponent<ActiveItem>(entity);
            AddComponent<LastActiveItem>(entity);
        }
    }
}
