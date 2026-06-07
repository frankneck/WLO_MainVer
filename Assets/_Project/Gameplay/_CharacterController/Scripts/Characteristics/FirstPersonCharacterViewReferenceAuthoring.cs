using UnityEngine;
using Unity.Entities;

public class FirstPersonCharacterViewReferenceAuthoring : MonoBehaviour
{
    public GameObject View;

    class Baker : Baker<FirstPersonCharacterViewReferenceAuthoring>
    {
        public override void Bake(FirstPersonCharacterViewReferenceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FirstPersonCharacterViewReference 
            { 
                ViewEntity = GetEntity(authoring.View, TransformUsageFlags.Dynamic)
            });
        }
    }
}