using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct NormalizeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var entity in SystemAPI.QueryBuilder()
                .WithAll<NeedNormalizationTag>()
                .Build()
                .ToEntityArray(Allocator.Temp))
        {
            if (!SystemAPI.HasBuffer<ParamRaw>(entity))
                continue;

            var rawBuffer = SystemAPI.GetBuffer<ParamRaw>(entity);

            float sum = 0f;
            for (int i = 0; i < rawBuffer.Length; i++)
            {
                sum += rawBuffer[i].Value;
            }

            if (sum > 0f)
            {
                for (int i = 0; i < rawBuffer.Length; i++)
                {
                    var v = rawBuffer[i];
                    v.Value /= sum;
                    rawBuffer[i] = v;
                }
            }

            SystemAPI.SetComponentEnabled<NeedNormalizationTag>(entity, false);
            SystemAPI.SetComponentEnabled<NeedQuantizationTag>(entity, true);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}