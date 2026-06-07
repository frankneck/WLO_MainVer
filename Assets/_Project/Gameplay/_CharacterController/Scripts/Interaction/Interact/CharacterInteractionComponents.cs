using Unity.Entities;

/// <summary>
/// Distance for character interaction. It needs to check that character can interact with interactable
/// </summary>
public struct CharacterInteractionDistance : IComponentData
{
    public float Value;
}

/// <summary>
/// Request to handle it in Job. It presents WHAT Interactes and WHAT is interactable 
/// </summary>
public struct InteractRequest : IComponentData
{
    public Entity Interacter;
    public Entity Interactable;
}

/// <summary>
/// Request to handle it in Job. It presents WHAT has buffer to add and WHAT is added 
/// </summary>
public struct AddToInventoryRequest : IComponentData
{
    public Entity Collector;
    public Entity Collectable;
}