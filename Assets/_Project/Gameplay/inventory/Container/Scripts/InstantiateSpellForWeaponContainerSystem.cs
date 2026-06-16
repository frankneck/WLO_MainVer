using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct InstantiateSpellForWeaponContainerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var jobHandle = new InstantiateSpellForWeaponContainerJob
        {
            SpawnedByLookup = SystemAPI.GetComponentLookup<SpawnerEntityReference>(true),
            
            WeaponSpellSetLookup = SystemAPI.GetBufferLookup<WeaponSpellSet>(true),
            SlotFillChanceLookup = SystemAPI.GetBufferLookup<SlotFillChance>(true),

            ContainerBufferLookup = SystemAPI.GetBufferLookup<ContainerBuffer>(),
            ECB = ecb
        };

        state.Dependency = jobHandle.Schedule(state.Dependency);
    } 
} 

[BurstCompile]
public partial struct InstantiateSpellForWeaponContainerJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<SpawnerEntityReference> SpawnedByLookup;
    [ReadOnly] public BufferLookup<WeaponSpellSet> WeaponSpellSetLookup;
    [ReadOnly] public BufferLookup<SlotFillChance> SlotFillChanceLookup;

    public BufferLookup<ContainerBuffer> ContainerBufferLookup;
    public EntityCommandBuffer ECB; 

    public void Execute(
        in FillWeaponContainer request,
        Entity entity
    )
    {        
        // Weapon
        var weaponEntity = request.Weapon;
        var weaponContainerEntity = request.Container; 
        var weaponContainerBuffer = ContainerBufferLookup[weaponContainerEntity];

        // Getting weapon component
        var spawnerEntity = SpawnedByLookup[weaponEntity].Entity;
        
        // Getting spell set buffer from spawner

        var spellSet = WeaponSpellSetLookup[spawnerEntity];
        var fillChance = SlotFillChanceLookup[spawnerEntity];
        
        if (spellSet.Length == 0)
        {
            ECB.DestroyEntity(entity);
            return;
        }
        float totalWeight = 0f;

        for (int i = 0; i < spellSet.Length; i++)
            totalWeight += spellSet[i].Weight;

        var rng = Random.CreateFromIndex((uint)entity.Index);

        for (int i = 0; i < weaponContainerBuffer.Length; i++)
        {
            float randomFillValue = rng.NextFloat(0f, 1f);

            //UnityEngine.Debug.Log($"iteration [{i}] [Random generator] randomFillValue={randomFillValue}; fillChance value={fillChance[i].Value}");
            
            if (randomFillValue > fillChance[i].Value)
                break;

            float randomSpellValue = rng.NextFloat(0f, totalWeight);

            float acc = 0f;
            Entity selectedSpell = Entity.Null; 

            for (int j = 0; j < spellSet.Length; j++)
            {
                acc += spellSet[j].Weight;

                if (randomSpellValue <= acc)
                {
                    selectedSpell = spellSet[j].PrefabEntity;
                    break;
                }
            }

            if (selectedSpell == Entity.Null)
                continue;

            var instantiatedSpell = ECB.Instantiate(selectedSpell);
            
            // Create request to add in weapon container
            var newRequest = ECB.CreateEntity();
            ECB.AddComponent(newRequest, new AddToContainer
            {
                Index = i,
                Item = instantiatedSpell,
                Container = weaponContainerEntity
            });
        }

        ECB.DestroyEntity(entity);
    } 
}

public struct AddToContainer : IComponentData
{
    public int Index;
    public Entity Item;
    public Entity Container;
}