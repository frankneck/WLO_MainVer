public interface IContainer
{
    Item Get(int index);
    void Set(Item item, int index);
    abstract bool CanPlace(Item item, Slot slot);
    public bool TryGetItem(int index, out Item item);
}    