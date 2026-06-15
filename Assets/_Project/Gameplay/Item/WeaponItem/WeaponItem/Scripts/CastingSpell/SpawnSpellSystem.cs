using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;

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
            BelongsTo = 1 << 5,             // RayCasts
            CollidesWith = 1 << 0 | 1 << 4, // GroundPlane  
        };
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
            .CollisionWorld;

        var jobHandle = new SpawnSpellJob
        {
            ECB = ecb,
            SelectionFilter = _selectionFilter,
            CollisionWorld = collisionWorld,
            Random = _random,

            SpreadLookup = SystemAPI.GetComponentLookup<WeaponSpread>(true),
            CastingSpellNumberLookup = SystemAPI.GetComponentLookup<WeaponCastSpellNumber>(true),
            WithWeaponContainerLookup = SystemAPI.GetComponentLookup<WithWeaponContainer>(true),
            ManaSpendBufferLookup = SystemAPI.GetBufferLookup<ManaSpendBuffer>(true),
            CurrentManaLookup = SystemAPI.GetComponentLookup<CurrentMana>(true),

            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(true),

            ProjectileEntityReferenceLookup = SystemAPI.GetComponentLookup<ProjectileEntityReference>(true),
            SpellTypeComponentLookup = SystemAPI.GetComponentLookup<SpellTypeComponent>(true),

            SpellDistanceLookup = SystemAPI.GetComponentLookup<SpellDistance>(true),
            ManaCostLookup = SystemAPI.GetComponentLookup<ManaCost>(true),
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
} 

[WithAll(typeof(Simulate))]
[BurstCompile]
public partial struct SpawnSpellJob : IJobEntity
{   
    public EntityCommandBuffer ECB;
    public CollisionFilter SelectionFilter;
    public CollisionWorld CollisionWorld;

    public Random Random;
    
    // Weapon item data
    [ReadOnly] public ComponentLookup<WeaponSpread> SpreadLookup;
    [ReadOnly] public ComponentLookup<WeaponCastSpellNumber> CastingSpellNumberLookup;
    [ReadOnly] public ComponentLookup<WithWeaponContainer> WithWeaponContainerLookup;
    [ReadOnly] public BufferLookup<ManaSpendBuffer> ManaSpendBufferLookup;
    [ReadOnly] public ComponentLookup<CurrentMana> CurrentManaLookup;

    // Container data
    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    // Choosed spell item data 
    [ReadOnly] public ComponentLookup<SpellTypeComponent> SpellTypeComponentLookup;
    [ReadOnly] public ComponentLookup<SpellDistance> SpellDistanceLookup;
    [ReadOnly] public ComponentLookup<ProjectileEntityReference> ProjectileEntityReferenceLookup;
    [ReadOnly] public ComponentLookup<ManaCost> ManaCostLookup;


    public void Execute(
        in ActiveItem activeWeapon,
        in OffsetForSpellSpawn spellSpawnOffset,
        in SelectedSpellToSpawn selectedSpellIndex,
        in LocalTransform characterTransform,
        GameTeam characterTeam,
        FirstPersonCharacterComponent characterComponent,
        Entity characterEntity
    )
    {
        if (!WithWeaponContainerLookup.HasComponent(activeWeapon.Entity)) 
            return;

        if (!ManaSpendBufferLookup.HasBuffer(activeWeapon.Entity))
            return;

        if (!CurrentManaLookup.HasComponent(activeWeapon.Entity)) 
            return;

        var weaponMana = CurrentManaLookup[activeWeapon.Entity];
        
        WithWeaponContainer weaponContainer = WithWeaponContainerLookup[activeWeapon.Entity]; 

        if (!ContainerBufferLookup.HasBuffer(weaponContainer.Container)) 
            return;

        var spellBuffer = ContainerBufferLookup[weaponContainer.Container];

        if (selectedSpellIndex.Value >= spellBuffer.Length)
            return;

        // Camera direction in the world
        quaternion cameraRotation = math.mul(characterTransform.Rotation, characterComponent.ViewLocalRotation);
        float3 cameraPosition = characterTransform.Position + new float3(0, 0.4f, 0); // TODO: CAMERA POSITION
        float3 spawnOffset = math.rotate(cameraRotation, spellSpawnOffset.Value);
        
        var bufferElement = spellBuffer[selectedSpellIndex.Value];
        Entity spellItemEntity = bufferElement.ItemEntity;

        if (!ProjectileEntityReferenceLookup.TryGetComponent(spellItemEntity, out var projectileReference))
            return;

        if (!SpellTypeComponentLookup.TryGetComponent(spellItemEntity, out var spellType))
            return;

        if (!SpellDistanceLookup.TryGetComponent(spellItemEntity, out var spellDistance))
            return;

        if (!ManaCostLookup.TryGetComponent(spellItemEntity, out var manaCost))
            return;
        
        if (!CastingSpellNumberLookup.TryGetComponent(activeWeapon.Entity, out var castSpellNumber))
            return;

        if (!SpreadLookup.TryGetComponent(activeWeapon.Entity, out var spread))
            return;

        float3 newPosForSpell = float3.zero;
        float3 forward = math.forward(cameraRotation);

        float remainingMana = weaponMana.Value;
        
        // Casting more than one spells if we need it 
        for (int i = 0; i < castSpellNumber.Value; i++)
        {       
            if (remainingMana < manaCost.Value) // if mana is less than manaCost skip spawn of spell entity 
                continue;

            remainingMana -= manaCost.Value;

            // Spread
            uint seed = (uint)(
                characterEntity.Index  ^
                selectedSpellIndex.Value  ^
                i  ^ (uint)selectedSpellIndex.FireTick.TickIndexForValidTick);
                
            Random random = Random.CreateFromIndex(seed);
            float3 moveDirection = ApplyRandomSpread(forward, spread.Value, ref random);

            // Getting where projectile starts and finishes 
            float3 startSpellPosition = characterTransform.Position + spawnOffset;
            float3 endSpellPosition = startSpellPosition + moveDirection * spellDistance.Value;

            // Getting raycast for calculation
            var selectionInput = new RaycastInput
            {
                Start = startSpellPosition,
                End = endSpellPosition,
                Filter = SelectionFilter,
            };

            var projectileEntity = Entity.Null;

            switch (spellType.Value)
            {
                case SpellType.MovingProjectile :
                    projectileEntity = SpawnAndSetMovingProjectile(
                        ECB, 
                        projectileReference, 
                        out newPosForSpell, 
                        cameraPosition, 
                        spawnOffset, 
                        moveDirection
                    );
                    break;
                case SpellType.StaticProjectile :  
                    projectileEntity = SpawnAndSetStaticProjectile(
                        ECB, 
                        projectileReference, 
                        out newPosForSpell,
                        selectionInput
                    );
                    break;
                default :
                    // If none - just nothing
                    break;
            }
            
            if (projectileEntity != Entity.Null)
            {
                SetCommonProjectileComponents(
                    ECB, 
                    projectileEntity,
                    characterTeam,
                    characterEntity,
                    newPosForSpell
                );

                ECB.AppendToBuffer(activeWeapon.Entity, new ManaSpendBuffer 
                { 
                    Value = manaCost.Value 
                });                 
            }
        }
        
        ECB.RemoveComponent<SelectedSpellToSpawn>(characterEntity);
    }
    
    /// Spread of casting spell
    private float3 ApplyRandomSpread(
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

    // Spawn and then setting a moving spell projectile entity
    private Entity SpawnAndSetMovingProjectile(
        EntityCommandBuffer ecb,
        ProjectileEntityReference projectileEntityReference,
        out float3 newPosForSpell,
        float3 cameraPosition,
        float3 spawnOffset,
        float3 moveDirection
    )
    {
        // Create projectile
        Entity isntantiatedEntity = ECB.Instantiate(projectileEntityReference.PrefabEntity);  

        ecb.SetComponent(isntantiatedEntity, new SpellDirection 
        { 
            Value = moveDirection 
        });

        newPosForSpell = cameraPosition + spawnOffset;

        return isntantiatedEntity;
    }

    // Spawn and then setting a static spell projectile entity
    private Entity SpawnAndSetStaticProjectile(
        EntityCommandBuffer ecb,
        ProjectileEntityReference projectileEntityReference,
        out float3 newPosForSpell,
        RaycastInput selectionInput
    )
    {
        // Create init values 
        Entity isntantiatedEntity = Entity.Null;
        newPosForSpell = float3.zero;

        if (CollisionWorld.CastRay(selectionInput, out var closestHit))
        {
            isntantiatedEntity = ecb.Instantiate(projectileEntityReference.PrefabEntity);  
            newPosForSpell = closestHit.Position;
        }

        return isntantiatedEntity;
    }

    private void SetCommonProjectileComponents(
        EntityCommandBuffer ecb,
        Entity projectileEntity,
        GameTeam characterTeam,
        Entity characterEntity,
        float3 newPosForSpell
    )
    {
        // Common projectile data
        ecb.SetComponent(projectileEntity, new GameTeam 
        { 
            Value = characterTeam.Value 
        }); 

        ecb.SetComponent(projectileEntity, new ProjectileCasterEntityReference 
        { 
            Entity = characterEntity 
        });

        LocalTransform newTransform = LocalTransform.FromPosition(newPosForSpell);

        ecb.SetComponent(projectileEntity, newTransform);
    }
}