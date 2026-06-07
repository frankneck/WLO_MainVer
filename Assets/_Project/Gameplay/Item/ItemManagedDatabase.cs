using System.Linq;
using UnityEngine;

public class ItemManagedDatabase : MonoBehaviour
{
    [SerializeField] private ItemScriptableDatabase source;
    private ItemDetails[] _items;

    public void Init()
    {
        _items = source.Items.ToArray();
    }

    public ref ItemDetails Get(ItemId id) 
    {
        return ref _items[id];
    }
}