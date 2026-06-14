using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class ProjectileAuthoring : MonoBehaviour
{
    public int ManaCost;
    public SpellType SpellType;
    [Range(0, 1000)]
    public float Distance;

    class SpellBaker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(entity, new ManaCost { Value = authoring.ManaCost });
            AddComponent(entity, new SpellTypeComponent { Value = authoring.SpellType });
            AddComponent(entity, new ProjectileDistance { Value = authoring.Distance });
            AddComponent<ProjectileOwner>(entity);
        }
    }
}