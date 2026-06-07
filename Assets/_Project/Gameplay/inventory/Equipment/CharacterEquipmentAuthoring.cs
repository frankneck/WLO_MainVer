using Unity.Entities;
using UnityEngine;

public class CharacterEquipmentAuthoring : MonoBehaviour
{
    class Baker : Baker<CharacterEquipmentAuthoring>
    {
        public override void Bake(CharacterEquipmentAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddBuffer<CharacterEquipment>(entity);
            AddComponent<CharacterEquipmentCashedVersion>(entity);
        }
    }
}