using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct SpellChoiceAndCoodownSystem : ISystem
{
    private int _simulationTickRate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        _simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        var jobHandle = new SpellChoiceAndCoodownJob
        {
            ECB = ecb,
            CurrentTick = currentTick,
            SimulationTickRate = _simulationTickRate,
            SimulationStepBatchSize = networkTime.SimulationStepBatchSize,

            ItemControlLookup = SystemAPI.GetComponentLookup<ItemControl>(true),
            WeaponCastDelayTargetTicksLookup = SystemAPI.GetBufferLookup<WeaponCastDelayTargetTicks>(false),
            
            IsShuffleLookup = SystemAPI.GetComponentLookup<WeaponShuffle>(true),
            CurrentManaLookup = SystemAPI.GetComponentLookup<CurrentMana>(true),
            WithWeaponContainer = SystemAPI.GetComponentLookup<WithWeaponContainer>(true),
            WeaponCastDelayLookup = SystemAPI.GetComponentLookup<WeaponCastDelay>(true),
            WeaponSpellStateLookup = SystemAPI.GetComponentLookup<StuffSpellState>(true),

            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(true),

            ProjectileReferenceLookup = SystemAPI.GetComponentLookup<ProjectileReference>(true),

            ManaCostLookup = SystemAPI.GetComponentLookup<ManaCost>(true),

            ManaSpendBufferLookup = SystemAPI.GetBufferLookup<ManaSpendBuffer>(true)
        };
        state.Dependency = jobHandle.Schedule(state.Dependency);
    }
}

[WithAll(typeof(WeaponTag))]
[BurstCompile]
public partial struct SpellChoiceAndCoodownJob : IJobEntity
{   
    public EntityCommandBuffer ECB;
    
    public NetworkTick CurrentTick;
    public int SimulationTickRate;
    public int SimulationStepBatchSize;
    
    // Item
    [ReadOnly] public ComponentLookup<ItemControl> ItemControlLookup;
    public BufferLookup<WeaponCastDelayTargetTicks> WeaponCastDelayTargetTicksLookup;

    [ReadOnly] public ComponentLookup<WeaponShuffle> IsShuffleLookup;
    [ReadOnly] public ComponentLookup<CurrentMana> CurrentManaLookup;
    [ReadOnly] public ComponentLookup<WithWeaponContainer> WithWeaponContainer;
    [ReadOnly] public ComponentLookup<WeaponCastDelay> WeaponCastDelayLookup;
    [ReadOnly] public ComponentLookup<StuffSpellState> WeaponSpellStateLookup;

    // Container buffer
    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    // Spell Item 
    [ReadOnly] public ComponentLookup<ProjectileReference> ProjectileReferenceLookup;
    
    // Spell Projectile
    [ReadOnly] public ComponentLookup<ManaCost> ManaCostLookup;
    
    [ReadOnly] public BufferLookup<ManaSpendBuffer> ManaSpendBufferLookup;

    [BurstCompile]
    public void Execute(
        in EquipedBy equipedByEntity,
        Entity weaponEntity
    )
    {        
        Entity characterEntity = equipedByEntity.Entity;

        if (characterEntity == Entity.Null)
            return;

        if (!ItemControlLookup.HasComponent(weaponEntity)) return;
        if (!WeaponCastDelayTargetTicksLookup.HasBuffer(weaponEntity)) return;

        if (!IsShuffleLookup.HasComponent(weaponEntity)) return;
        if (!CurrentManaLookup.HasComponent(weaponEntity)) return;
        if (!WeaponSpellStateLookup.HasComponent(weaponEntity)) return;
        if (!WeaponCastDelayLookup.HasComponent(weaponEntity)) return;
        if (!WithWeaponContainer.HasComponent(weaponEntity)) return;

        var delayTargetTicksBuffer = WeaponCastDelayTargetTicksLookup[weaponEntity];
        var control = ItemControlLookup[weaponEntity];

        var isShuffle = IsShuffleLookup[weaponEntity];
        var mana = CurrentManaLookup[weaponEntity];
        var spellState = WeaponSpellStateLookup[weaponEntity];
        var delay = WeaponCastDelayLookup[weaponEntity];

        var weaponContainer = WithWeaponContainer[weaponEntity].Container;
        
        var spellBuffer = ContainerBufferLookup[weaponContainer];

        bool isOnDelay = true;
        var curAbilityTargetTick = new WeaponCastDelayTargetTicks();

        uint delayInTicks = (uint)(SimulationTickRate * delay.Value);

        for (var i = 0u; i < SimulationStepBatchSize; i++)
        {
            var testTick = CurrentTick;
            testTick.Subtract(i);

            // If buffer is clean
            if (!delayTargetTicksBuffer.GetDataAtTick(testTick, out curAbilityTargetTick))
            {
                curAbilityTargetTick.Value = NetworkTick.Invalid;
            }

            // if buffer is clean or buffer in this Test Tick is more than current tick
            if (curAbilityTargetTick.Value == NetworkTick.Invalid ||
                !curAbilityTargetTick.Value.IsNewerThan(CurrentTick))
            {
                isOnDelay = false;
                break;
            }
        }
        
        // If is delay skip the player intention to attack because he mustn't
        if (isOnDelay)
        {
            return;
        }

        // but if not check Tick when the Player isSet (1 Tick)  
        if (control.MainActionPressed)
        {
            if (spellBuffer.Length <= 0)
            {
                return;
            }

            // Getting spell from slots
            int originalIndex = GetSpellIndex(isShuffle.Value, ref spellState, spellBuffer);
            int index = originalIndex;
            Entity spellItem = Entity.Null;

            for (int i = 0; i < spellBuffer.Length; i++)
            {
                var candidate = spellBuffer[index].ItemEntity;
                
                if (candidate != Entity.Null)
                {
                    spellItem = candidate;
                    break;
                }
                
                index = (index + 1) % spellBuffer.Length;
            }

            // Write 
            ECB.SetComponent(weaponEntity, spellState);

            if (spellItem == Entity.Null)
            {
                return;
            }

            var projectile = ProjectileReferenceLookup[spellItem].PrefabEntity;
            var manaCost = ManaCostLookup[projectile];

            if (mana.Value < manaCost.Value) // if mana is 0 skip spell spawningn
                return;

            if (!ManaSpendBufferLookup.HasBuffer(weaponEntity))
                return;

            ECB.AppendToBuffer( weaponEntity, new ManaSpendBuffer 
            { 
                Value = manaCost.Value 
            }); 

            ECB.AddComponent(characterEntity, new SpawnSpellRequest
            {
                FireTick = CurrentTick,
                Index = index
            });

            var newCooldownTargetTick = CurrentTick;
            newCooldownTargetTick.Add(delayInTicks);
            curAbilityTargetTick.Value = newCooldownTargetTick;

            var nextTick = CurrentTick;
            nextTick.Add(1u);
            curAbilityTargetTick.Tick = nextTick;

            delayTargetTicksBuffer.AddCommandData(curAbilityTargetTick);
        }
    }

    /// <summary>
    /// Getting spell index to cast next. shuffle is random
    /// </summary>
    private int GetSpellIndex(
        bool shuffle, 
        ref StuffSpellState state, 
        DynamicBuffer<ContainerBuffer> buffer)
    {    
        int index;
        int realsize = GetRealSize(buffer);

        if (shuffle)
        {
            var newRandom = state.Random;
            index = newRandom.NextInt(realsize);
            state.Random = newRandom;
        }
        else
        {
            if (realsize <= 0)
            {
                return 0;
            }

            index = state.LastIndex;
            index = (index + 1) % realsize;
            state.LastIndex = index;
        }

        return index;
    }

    /// <summary>
    /// Allow to get real size of buffer
    /// </summary>
    private int GetRealSize(DynamicBuffer<ContainerBuffer> buffer)
    {
        int realSize = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].ItemEntity != Entity.Null)
            {
                realSize++;
            }
        }

        return realSize;
    }
}

// WeaponItemEntity содержит Control
// При нажатии изменяется Control 