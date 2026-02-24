using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class SpellAuthoring : MonoBehaviour
{
    [Header("AoeSpell")]
    public GameObject AoeSpell;
    public float AoeSpellCooldown;
    
    [Header("SkillShotSpell")]
    public GameObject SkillShotSpell;
    public float SkillShotSpellCooldown;
    public float SpellMoveSpeed;

    class SpellBaker : Baker<SpellAuthoring>
    {
        public override void Bake(SpellAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new SpellPrefabs
            {
                AoeAbility = GetEntity(authoring.AoeSpell, TransformUsageFlags.Dynamic),
                SkillShotAbility = GetEntity(authoring.SkillShotSpell, TransformUsageFlags.Dynamic),
            });
            AddComponent(entity, new SpellCooldown { 
                AoeAbility = authoring.AoeSpellCooldown,
                SkillShotAbility = authoring.SkillShotSpellCooldown 
            });
            AddBuffer<SpellCooldownTargetTicks>(entity);
        }
    }
}