using UnityEngine;
using Unity.Entities;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class FirstPersonPlayerAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public float LookInputSensitivity = 0.2f;

    public class Baker : Baker<FirstPersonPlayerAuthoring>
    {
        public override void Bake(FirstPersonPlayerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);

            AddComponent<PlayerTag>(entity);
            
            AddComponent(entity, new FirstPersonPlayer
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                LookInputSensitivity = authoring.LookInputSensitivity,
            });
            
            AddComponent<FirstPersonPlayerCommands>(entity);
            AddComponent<FirstPersonPlayerNetworkInput>(entity);

            AddComponent<PlayerPing>(entity);

            AddComponent<SelectedSlotIndex>(entity);

            // Respawn
            AddComponent<LeftSecondsToRespawn>(entity);

            AddComponent(entity, new CurrentPlayerState
            {
                Value = PlayerState.None
            });

            AddComponent<InputPermissions>(entity);

            AddComponent<BelongsToMatchId>(entity);
            AddComponent<BelongsToMatch>(entity);
            AddComponent<NetworkEntityReference>(entity);
        }
    }
}

