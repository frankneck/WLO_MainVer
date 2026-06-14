using UnityEngine;
using Unity.Entities;

public class CharacterNameAuthroing : MonoBehaviour
{
    public string Name = "";

    class CharacterBaker : Baker<CharacterNameAuthroing>
    {
        public override void Bake(CharacterNameAuthroing authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new CharacterName { Value = authoring.Name });
        }
    }
}