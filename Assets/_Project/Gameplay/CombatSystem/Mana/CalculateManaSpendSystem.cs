using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CalculateManaSpendSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }
    
    public void OnUpdate(ref SystemState state)
    {        
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var currentTick = networkTime.ServerTick;

        foreach (var (manaSpendBuffer, manaSpendThisTickBuffer) in SystemAPI
            .Query<DynamicBuffer<ManaSpendBuffer>, DynamicBuffer<ManaSpendThisTickBuffer>>())
        {
            if (manaSpendBuffer.IsEmpty)
            {
                manaSpendThisTickBuffer.AddCommandData(new ManaSpendThisTickBuffer 
                { 
                    Tick = currentTick, 
                    Value = 0 
                });
            }
            else
            {
                float total = 0;

                if (manaSpendThisTickBuffer.GetDataAtTick(currentTick, out var manaSpendThisTick))
                {
                    total = manaSpendThisTick.Value;
                }
                
                foreach (var manaSpend in manaSpendBuffer)
                {
                    total += manaSpend.Value;
                }

                manaSpendThisTickBuffer.AddCommandData(new ManaSpendThisTickBuffer 
                { 
                    Tick = currentTick, 
                    Value = total 
                });
                
                manaSpendBuffer.Clear();
            }
        }
    }
}