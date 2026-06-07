using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// Detects interactables (just interact or pickup)
/// </summary>
[WorldSystemFilter((WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation))]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[BurstCompile]
public partial struct TooltipDetectionSystem : ISystem
{
    private CollisionFilter _selectionFilter;

    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ClientCurrentObservedObject>();

        _selectionFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5,
            CollidesWith = 1 << 7
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var tooltipStateRW = SystemAPI.GetSingletonRW<ClientCurrentObservedObject>(); 
        var collisionWorld = SystemAPI
            .GetSingleton<PhysicsWorldSingleton>()
            .CollisionWorld;

        foreach (var (transform, character, distance) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRO<FirstPersonCharacterComponent>, 
                RefRO<CharacterInteractionDistance>>()
            .WithAll<LocalCharacterTag>())
        {
            float3 forwardLocal = math.forward(character.ValueRO.ViewLocalRotation);
            float3 forwardWorld = math.mul(transform.ValueRO.Rotation, forwardLocal);

            float3 start = transform.ValueRO.Position + new float3(0f, 0.4f, 0f);
            float3 end = start + forwardWorld * distance.ValueRO.Value; // дистанция

            var input = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = _selectionFilter
            };

            var tooltip = tooltipStateRW.ValueRW;

            bool hasHit = collisionWorld.CastRay(input, out var hit);

            tooltip.IsVisible = hasHit;
            tooltip.Target = hasHit ? hit.Entity : Entity.Null;
            tooltip.IsCollectable = hasHit && SystemAPI.HasComponent<CurrentPickupMode>(hit.Entity);

            SystemAPI.SetSingleton(tooltip);
        }   
    }
}