using Unity.Entities;
using UnityEngine;

/// <summary>
/// Adds neccessery components to character to interact with interactables. For example you can add this authoring to player.  
/// </summary>
public class CharacterInteractionControlAuthoring : MonoBehaviour
{
    [SerializeField] private float Distance = 1; 

    class Baker : Baker<CharacterInteractionControlAuthoring>
    {
        public override void Bake(CharacterInteractionControlAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<CharacterInteractionControl>(entity);
            AddComponent(entity, new CharacterInteractionDistance { Value = authoring.Distance });
        }
    }
}

