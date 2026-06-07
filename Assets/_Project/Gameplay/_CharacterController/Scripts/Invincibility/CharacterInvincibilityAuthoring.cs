using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class CharacterInvincibilityAuthoring : MonoBehaviour
{
    public float InvincibilityTimer;

    class RespawnEntityBaker : Baker<CharacterInvincibilityAuthoring>
    {
        public override void Bake(CharacterInvincibilityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new InvincibilityTimer { Value = authoring.InvincibilityTimer });
            AddComponent(entity, new DamageMultiplier { Multiplier = 1 });
        }
    }
}