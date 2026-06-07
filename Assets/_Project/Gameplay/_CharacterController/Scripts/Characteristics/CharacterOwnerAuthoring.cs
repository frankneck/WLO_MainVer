using Unity.Entities;
using UnityEngine;

public class CharacterOwnerAuthoring : MonoBehaviour
{
    class DamageOnTriggerBaker : Baker<CharacterOwnerAuthoring>
    {
        public override void Bake(CharacterOwnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<CharacterOwner>(entity);
        }
    }
}