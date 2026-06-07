using UnityEngine;
using Unity.Entities;

public class PlayerCharacterAuthoring : MonoBehaviour
{
    class CharacterBaker : Baker<PlayerCharacterAuthoring>
    {
        public override void Bake(PlayerCharacterAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<PlayerCharacterTag>(entity);
            AddComponent<NewCharacterPlayerTag>(entity);
        }
    }
}