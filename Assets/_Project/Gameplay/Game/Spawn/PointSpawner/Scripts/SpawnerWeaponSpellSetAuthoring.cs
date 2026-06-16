using Unity.Entities;
using UnityEngine;

/// <summary>
/// Attach to weapon point spawner
/// </summary>
class SpawnerWeaponSpellSetAuthoring : MonoBehaviour
{
    [SerializeField] private WeaponSpellSetConfig m_Config; 

    class WeaponSpellSetAuthoringBaker : Baker<SpawnerWeaponSpellSetAuthoring>
    {
        public override void Bake(SpawnerWeaponSpellSetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            var config = authoring.m_Config;
            
            // Spell pool

            var spellBuffer = AddBuffer<WeaponSpellSet>(entity);

            foreach (var e in config.Spells)
            {
                var spellSet = new WeaponSpellSet
                {
                    PrefabEntity = GetEntity(e.SpellPrefab, TransformUsageFlags.Dynamic),
                    Weight = e.Weight 
                };

                spellBuffer.Add(spellSet);
            }

            // Chance to fill slot 

            var fillBuffer = AddBuffer<SlotFillChance>(entity);

            for (int i = 0; i < config.MaxSlots; i++)
            {
                float chance = config.GetFillChance(i);

                fillBuffer.Add(new SlotFillChance
                {
                    Value = Mathf.Clamp01(chance)
                });
            }
        }
    }
}

