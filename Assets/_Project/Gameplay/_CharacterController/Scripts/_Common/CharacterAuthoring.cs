using UnityEngine;
using Unity.Entities;

public class CharacterAuthroing : MonoBehaviour
{
    class CharacterBaker : Baker<CharacterAuthroing>
    {
        public override void Bake(CharacterAuthroing authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<CashedCharacterData>(entity);
            AddComponent<CharacterTag>(entity);
        }
    }
}