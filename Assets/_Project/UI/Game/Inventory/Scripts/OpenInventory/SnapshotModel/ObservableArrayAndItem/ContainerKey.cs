using Unity.Entities;

public struct ContainerKey
{
    public Entity Owner;
    public SlotType Type;

    public bool Equals(ContainerKey key)
    {
        if (
            Owner == key.Owner
            && Type == key.Type)
        {
            return true;
        }

        return false;
    }
} 