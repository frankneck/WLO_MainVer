using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct SpawnSpellSystem : ISystem
{
    private CollisionFilter _selectionFilter;
    private Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {                
        _selectionFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5,     // RayCasts
            CollidesWith = 1 << 0, // GroundPlane  
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        var jobHandle = new SpawnSpellJob
        {
            ECB = ecb,
            SelectionFilter = _selectionFilter,
            CollisionWorld = collisionWorld,
            Random = _random,

            SpreadLookup = SystemAPI.GetComponentLookup<WeaponSpread>(true),
            CastingSpellNumberLookup = SystemAPI.GetComponentLookup<WeaponCastSpellNumber>(true),
            WeaponContainerLookup = SystemAPI.GetComponentLookup<WithWeaponContainer>(true),

            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(true),

            ProjectileLookup = SystemAPI.GetComponentLookup<ProjectileReference>(true),
            SpellTypeComponentLookup = SystemAPI.GetComponentLookup<SpellTypeComponent>(true),

            ProjectileDistanceLookup = SystemAPI.GetComponentLookup<ProjectileDistance>(true)
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
} 

[BurstCompile]
public partial struct SpawnSpellJob : IJobEntity
{   
    // Common
    public EntityCommandBuffer ECB;
    public CollisionFilter SelectionFilter;
    public CollisionWorld CollisionWorld;

    public Random Random;
    
    // Weapon component lookups
    [ReadOnly] public ComponentLookup<WeaponSpread> SpreadLookup;
    [ReadOnly] public ComponentLookup<WeaponCastSpellNumber> CastingSpellNumberLookup;
    [ReadOnly] public ComponentLookup<WithWeaponContainer> WeaponContainerLookup;

    // Container buffer lookups
    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    // Projectilve lookup
    [ReadOnly] public ComponentLookup<ProjectileReference> ProjectileLookup;
    [ReadOnly] public ComponentLookup<SpellTypeComponent> SpellTypeComponentLookup;

    [ReadOnly] public ComponentLookup<ProjectileDistance> ProjectileDistanceLookup;


    public void Execute(
        in LocalTransform transform,
        in ActiveItem currentStuff,
        in OffsetForSpellSpawn offset,
        in SpawnSpellRequest request,
        GameTeam playerTeam,
        FirstPersonCharacterComponent characterComponent,
        Simulate simulate,
        Entity entity
    )
    {
        if (!WeaponContainerLookup.TryGetComponent(currentStuff.Entity, out var weaponContainer))
            return;

        if (!ContainerBufferLookup.TryGetBuffer(weaponContainer.Container, out var spellBuffer))
            return;

        if (request.Index >= spellBuffer.Length)
            return; 

        // Camera direction in the world
        quaternion cameraRotation = math.mul(transform.Rotation, characterComponent.ViewLocalRotation);
        float3 cameraPosition = transform.Position + new float3(0, 0.4f, 0); // TODO: CAMERA POSITION
        float3 spawnOffset = math.rotate(cameraRotation, offset.Value);
        
        var spellItem = spellBuffer[request.Index].ItemEntity;
        if (!ProjectileLookup.TryGetComponent(spellItem, out var projectileReference))
            return;

        if (!SpellTypeComponentLookup.TryGetComponent(projectileReference.PrefabEntity, out var spellType))
            return;

        if (!CastingSpellNumberLookup.TryGetComponent(currentStuff.Entity, out var castSpellNumber))
            return;

        // Casting more than one spells if we need it 
        for (int i = 0; i < castSpellNumber.Value; i++)
        {
            var entitySpell = ECB.Instantiate(projectileReference.PrefabEntity);  

            ECB.SetComponent(entitySpell, new GameTeam 
            { 
                Value = playerTeam.Value 
            }); 
            ECB.SetComponent(entitySpell, new ProjectileOwner 
            { 
                Entity = entity 
            });
            
            float3 newPos = float3.zero;
            float3 forward = math.forward(cameraRotation);

            if (!SpreadLookup.TryGetComponent(currentStuff.Entity, out var spread))
                return;

            // Scatter
            uint seed = (uint)(
                entity.Index  ^
                request.Index  ^
                i  ^ (uint)request.FireTick.TickIndexForValidTick);
                
            Random random = Random.CreateFromIndex(seed);
            float3 spellMoveDirection = ApplyRandomSpread(forward, spread.Value, ref random);

            // if current spell is skillshot - assgin spell move direction 
            if (spellType.Value == SpellType.SkillShot)
            {
                ECB.SetComponent(entitySpell, new SpellDirection 
                { 
                    Value = spellMoveDirection 
                });

                newPos = cameraPosition + spawnOffset;
            }
            else if (spellType.Value == SpellType.AoeSpell)
            {
                if (!ProjectileDistanceLookup.TryGetComponent(projectileReference.PrefabEntity, out var distance))
                    return;

                float3 start = transform.Position + spawnOffset;
                float3 end = start + spellMoveDirection * distance.Value;

                var selectionInput = new RaycastInput
                {
                    Start = start,
                    End = end,
                    Filter = SelectionFilter,
                };

                if (CollisionWorld.CastRay(selectionInput, out var closestHit))
                {
                    newPos = closestHit.Position;
                }
                else
                {
                    ECB.DestroyEntity(entitySpell);
                    entitySpell = Entity.Null;  
                }
            }

            else if (spellType.Value == SpellType.None) 
                return;
            
            if (entitySpell == Entity.Null) 
                return;

            quaternion newRot = cameraRotation;
            LocalTransform newTransform = LocalTransform.FromPositionRotation(newPos, newRot);

            ECB.SetComponent(entitySpell, newTransform);
            ECB.RemoveComponent<SpawnSpellRequest>(entity);
        }
    }
    
    /// <summary>
    /// Spread of casting spell
    /// </summary>
    float3 ApplyRandomSpread(
        float3 forward, 
        float spreadAngle,
        ref Random random)
    {
        float3 right = math.normalize(math.cross(forward, math.up()));
    
        // if right equals (0,1,0) change to (1,0,0)
        if (math.lengthsq(right) < 0.001f)
        {
            right = math.normalize(math.cross(forward, math.right()));
        }

        float3 up = math.cross(forward, right);

        // AZIMUT
        float angle = random.NextFloat(0, math.PI * 2f);

        // VALUE OF AZIMUT
        float r = random.NextFloat(0, 1f);
        float spreadScale = r * math.tan(math.radians(spreadAngle)); 
    
        float x = math.cos(angle) * spreadScale;
        float y = math.sin(angle) * spreadScale;
    
        float3 finalDirection = forward + right * x + up * y; 
        return math.normalize(finalDirection);
    }
}