using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(InitCollectableItemSpawnerSystem))]
[BurstCompile]
public partial struct SpawnedWeaponInitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var job = new SpawnedWeaponInitJob
        {
            SpawnerWeaponParametersSetLookup = SystemAPI.GetBufferLookup<SpawnerParamSet>(),
            ParamSetLookup = SystemAPI.GetBufferLookup<WeaponParamSet>(),
            ItemLevelLookup = SystemAPI.GetComponentLookup<CurrentWeaponLevel>(),
            ECB = ecb  
        };
        
        state.Dependency = job.Schedule(state.Dependency);

    } 
}  

[BurstCompile]
public partial struct SpawnedWeaponInitJob : IJobEntity
{
    public BufferLookup<SpawnerParamSet> SpawnerWeaponParametersSetLookup;
    public BufferLookup<WeaponParamSet> ParamSetLookup;
    public ComponentLookup<CurrentWeaponLevel> ItemLevelLookup;
    
    public EntityCommandBuffer ECB;

    public void Execute(
        AssignLevelRequest request,
        Entity entity)
    {
        var spawnerEntity = request.SpawnerEntity;
        var weaponEntity = request.SpawnedEntity;

        var bufferData = SpawnerWeaponParametersSetLookup[spawnerEntity];

        ECB.SetComponent(weaponEntity, new CurrentWeaponLevel 
        { 
            Value = request.Level 
        });

        var weaponParamSet = ParamSetLookup[weaponEntity];
        weaponParamSet.Clear();

        foreach (var e in bufferData)
        {
            var newParamSetValue = new WeaponParamSet
            {
                Id = e.Id,
                Type = e.Type,
                Threshold = e.Threshold,
                Step = e.Step,
                MinValue = e.MinValue,
                MaxValue = e.MaxValue
            };

            weaponParamSet.Add(newParamSetValue);
        }

        // Next step in pipeline
        ECB.SetComponentEnabled<NeedRawGenerationTag>(weaponEntity, true);
        
        ECB.DestroyEntity(entity);
    }
}