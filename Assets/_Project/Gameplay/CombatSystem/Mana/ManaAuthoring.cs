using UnityEngine;
using Unity.Entities;

public class ManaAuthoring : MonoBehaviour
{
    public class Baker : Baker<ManaAuthoring>
    {
        public override void Bake(ManaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<WeaponMaxMana>(entity);
            AddComponent<CurrentMana>(entity);
            
            AddComponent<WeaponManaRecoveryRate>(entity);
            AddBuffer<ManaSpendBuffer>(entity);
            
            AddBuffer<ManaSpendThisTickBuffer>(entity);
            AddComponent<AccumulatedMana>(entity);
        }
    }
}

