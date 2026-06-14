using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct QuantizeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var entity in SystemAPI.QueryBuilder()
                .WithAll<NeedQuantizationTag>()
                .Build()
                .ToEntityArray(Allocator.Temp))
        {
            if (!SystemAPI.HasBuffer<ParamRaw>(entity) 
                || !SystemAPI.HasBuffer<WeaponParamSet>(entity))
                continue;

            var rawBuffer = SystemAPI.GetBuffer<ParamRaw>(entity);
            var setBuffer = SystemAPI.GetBuffer<WeaponParamSet>(entity);

            // Проходим по сырым значениям каждого параметра
            for (int i = 0; i < rawBuffer.Length; i++)
            {   
                // Берем настройки (порог, максимальное значение, минимальное и тд) 
                var meta = setBuffer[i];

                float original = rawBuffer[i].Value;
                float quantized = original;

                // В зависимости от типа делаем квантизацию
                switch (meta.Type)
                {
                    case ParameterType.Bool:
                        quantized = original >= meta.Threshold ? 1f : 0f;
                        break;

                    case ParameterType.Int:
                        quantized = math.floor(original / meta.Step) * meta.Step;
                        break;

                    case ParameterType.Float:
                        quantized = original;
                        break;
                }

                quantized = math.clamp(quantized, meta.MinValue, meta.MaxValue);
                
                // Write param value
                rawBuffer[i] = new ParamRaw
                {
                    Id = rawBuffer[i].Id,
                    Value = quantized
                };

                // Дальше распределяются числа, 
                // не вошедвшие в квантизированное по всем параметрам

                // calculate delta
                var delta =  math.abs(original - quantized);
                if (delta <= 0f)
                    continue;

                float totalFree = 0f;

                // calculate total free of other values
                for (int j = i + 1; j < rawBuffer.Length; j++)
                {
                    var m = setBuffer[j];

                    float free = m.MaxValue - rawBuffer[j].Value;

                    if (free > 0f)
                        totalFree += free;
                }

                if (totalFree <= 0f)
                    continue;

                // distribute delta value for other params
                for (int j = i + 1; j < rawBuffer.Length; j++)
                {
                    var m = setBuffer[j];

                    float free = m.MaxValue - rawBuffer[j].Value;

                    if (free <= 0f)
                        continue;

                    float share = free / totalFree;
                    float add = delta * share;

                    var v = rawBuffer[j];
                    v.Value = math.clamp(v.Value + add, m.MinValue, m.MaxValue);
                    rawBuffer[j] = v;
                    
                    
                }
            }

            SystemAPI.SetComponentEnabled<NeedQuantizationTag>(entity, false);
            SystemAPI.SetComponentEnabled<NeedApplyFinalValuesTag>(entity, true);
        }
    }
}