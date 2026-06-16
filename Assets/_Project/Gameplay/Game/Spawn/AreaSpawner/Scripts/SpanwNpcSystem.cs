using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct SpawnNpcSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (number, radius, random, spawnerState, spawnPrefab, transform) in SystemAPI
            .Query<RefRO<NumberEntitiesToSpawn>, RefRO<SpawnRadius>, RefRW<RadiusRandom>, 
                RefRW<CurrentSpawnerState>, RefRO<SpawnerTargetEntity>, RefRO<LocalTransform>>()
            .WithAll<SpawnerTag>())
        {

            // TODO: Add number of spawn 
            if (spawnerState.ValueRW.Value == SpawnerState.Disactive) continue;

            for (int i = 0; i < number.ValueRO.Value; i++)
            {
                random.ValueRW.Value = Random.CreateFromIndex((uint)i);
                var instanciatedEntity = ecb.Instantiate(spawnPrefab.ValueRO.PrefabEntity);

                float3 spawnerPosition = transform.ValueRO.Position;
                float3 newPosition = new float3(spawnerPosition.x + random.ValueRO.Value.NextFloat(radius.ValueRO.Value),
                                                spawnerPosition.y,
                                                spawnerPosition.z + random.ValueRO.Value.NextFloat(radius.ValueRO.Value));
                
                LocalTransform npcNewTransform = LocalTransform.FromPosition(newPosition);
                ecb.SetComponent(instanciatedEntity, npcNewTransform);

                spawnerState.ValueRW.Value = SpawnerState.Disactive;

                UnityEngine.Debug.Log($"[SpawnNpcSystem] The spawner processed");
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}