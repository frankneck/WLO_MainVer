using UnityEngine;
using Unity.Entities;

public class CharacterSocketsAuthoring : MonoBehaviour
{
    [Header("First person sockets")]
    [SerializeField] private Transform FPV_Socket;
    
    [Header("Third person sockets")]
    [SerializeField] private Transform TPV_Socket;

    class CharacterSocketsBaker : Baker<CharacterSocketsAuthoring>
    {
        public override void Bake(CharacterSocketsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FirstPersonCharacterSocket
            {
                Entity = GetEntity(authoring.FPV_Socket, TransformUsageFlags.Dynamic),
            });
            
            AddComponent(entity, new ThirdPersonCharacterSocket
            {
                Entity = GetEntity(authoring.TPV_Socket, TransformUsageFlags.Dynamic),
            });
        }
    }
}