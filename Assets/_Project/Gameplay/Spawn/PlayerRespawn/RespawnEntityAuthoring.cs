using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class RespawnEntityAuthoring : MonoBehaviour
{
    public float RespawnTime;
    public NetCodeConfig NetCodeConfig;
    public int SimulationTickRate;

    class RespawnEntityBaker : Baker<RespawnEntityAuthoring>
    {
        public override void Bake(RespawnEntityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            authoring.SimulationTickRate = authoring.NetCodeConfig.ClientServerTickRate.SimulationTickRate;
            
            AddComponent<RespawnEntityTag>(entity);
            AddComponent(entity, new RespawnTickCount
            {
                Value = (uint)(authoring.SimulationTickRate * authoring.RespawnTime) 
            });
            
            AddBuffer<RespawnElementBuffer>(entity);
        }
    }
}