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

            ProjectileReferenceLookup = SystemAPI.GetComponentLookup<ProjectileEntityReference>(true),

            ManaCostLookup = SystemAPI.GetComponentLookup<ManaCost>(true),
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
    
    // Weapon item
    [ReadOnly] public ComponentLookup<ItemControl> ItemControlLookup;
    [ReadOnly] public ComponentLookup<WeaponShuffle> IsShuffleLookup;
    [ReadOnly] public ComponentLookup<CurrentMana> CurrentManaLookup;
    [ReadOnly] public ComponentLookup<WithWeaponContainer> WithWeaponContainer;
    [ReadOnly] public ComponentLookup<WeaponCastDelay> WeaponCastDelayLookup;
    [ReadOnly] public ComponentLookup<StuffSpellState> WeaponSpellStateLookup;
    public BufferLookup<WeaponCastDelayTargetTicks> WeaponCastDelayTargetTicksLookup;

    // Container buffer
    [ReadOnly] public BufferLookup<ContainerBuffer> ContainerBufferLookup;

    // Spell Item 
    [ReadOnly] public ComponentLookup<ProjectileEntityReference> ProjectileReferenceLookup;
    [ReadOnly] public ComponentLookup<ManaCost> ManaCostLookup;

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

        // Getting weapon item data
        var weaponControl = ItemControlLookup[weaponEntity];
        var weaponIsShuffle = IsShuffleLookup[weaponEntity];
        var weaponMana = CurrentManaLookup[weaponEntity];
        var weaponSpellState = WeaponSpellStateLookup[weaponEntity];
        var weaponDelay = WeaponCastDelayLookup[weaponEntity];
        var weaponContainer = WithWeaponContainer[weaponEntity].Container;
        
        var delayTargetTicksBuffer = WeaponCastDelayTargetTicksLookup[weaponEntity];
        
        if (!ContainerBufferLookup.HasBuffer(weaponContainer))
            return;

        // Getting weapon container data
        var weaponContainerBuffer = ContainerBufferLookup[weaponContainer];

        bool isOnDelay = true;
        var curAbilityTargetTick = new WeaponCastDelayTargetTicks();

        uint delayInTicks = (uint)(SimulationTickRate * weaponDelay.Value);

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
        if (weaponControl.MainActionPressed)
        {
            if (weaponContainerBuffer.Length <= 0)
            {
                return;
            }

            // Getting spell index from buffer
            int originalIndex = GetSpellIndex(
                weaponIsShuffle.Value, 
                ref weaponSpellState, 
                weaponContainerBuffer
            );

            int index = originalIndex;
            Entity spellItem = Entity.Null;

            for (int i = 0; i < weaponContainerBuffer.Length; i++)
            {
                var candidate = weaponContainerBuffer[index].ItemEntity;
                
                if (candidate != Entity.Null)
                {
                    spellItem = candidate;
                    break;
                }
                
                index = (index + 1) % weaponContainerBuffer.Length;
            }

            // Write 
            ECB.SetComponent(weaponEntity, weaponSpellState);

            if (spellItem == Entity.Null)
                return;

            if (!ManaCostLookup.HasComponent(spellItem))
                return;

            ManaCost manaCost = ManaCostLookup[spellItem];

            if (weaponMana.Value < manaCost.Value) // if mana is 0 skip spell spawningn
                return;

            ECB.AddComponent(characterEntity, new SelectedSpellToSpawn
            {
                FireTick = CurrentTick,
                Value = index
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