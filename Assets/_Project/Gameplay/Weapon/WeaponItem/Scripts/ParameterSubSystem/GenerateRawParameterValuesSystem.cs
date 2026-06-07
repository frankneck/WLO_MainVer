using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GenerateXSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (rawBuffer, paramSet, entity) in SystemAPI
            .Query<DynamicBuffer<ParamRaw>, DynamicBuffer<WeaponParamSet>>()
            .WithAll<NeedRawGenerationTag>()
            .WithEntityAccess())
        {
            // rawBuffer.Clear();
            
            for (int i = 0; i < paramSet.Length; i++)
            {
                var rand = Random.CreateFromIndex((uint)(entity.Index + i));
                
                var param = paramSet[i];
                float x = rand.NextFloat(0f, 1f);

                rawBuffer.Add(new ParamRaw
                {
                    Id = param.Id,
                    Value = x
                });
            }
            
            SystemAPI.SetComponentEnabled<NeedRawGenerationTag>(entity, false);
            SystemAPI.SetComponentEnabled<NeedNormalizationTag>(entity, true);
        }
    }
} 