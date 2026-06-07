using UnityEngine;
using Unity.Entities;

/// <summary>
/// Adds main component for container
/// </summary>
public class CharacterContainersAuthoring : MonoBehaviour
{
    [SerializeField] private InventoryConfig m_Config;

    public class Baker : Baker<CharacterContainersAuthoring>
    {
        public override void Bake(CharacterContainersAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            var config = authoring.m_Config;       
            
            AddComponent(entity, new CharacterContainersCapacity
            {
                WeaponEquipmentSize = config.WeaponEquipmentMaxCapacity,
                ConsumableEquipmentSize = config.ConsumableEquipmentMaxCapacity,
                BackpackSize = config.InventoryMaxCapacity
            });

            AddComponent<WithCharacterContainers>(entity);

            AddComponent<NeedToCreateContainer>(entity);
            
            SetComponentEnabled<NeedToCreateContainer>(entity, false);
        }
    }
}

/// <summary>
/// Defines max capacity for player character containers
/// </summary>
public struct CharacterContainersCapacity : IComponentData
{
    public int WeaponEquipmentSize;
    public int ConsumableEquipmentSize;
    public int BackpackSize;
}