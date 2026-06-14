using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory/ItemDataBase")]
public class ItemScriptableDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDetails> _items;
    public IReadOnlyList<ItemDetails> Items => _items;


#if UNITY_EDITOR
    [ContextMenu("Assign IDs")]
    private void AssignId()
    {
        HashSet<ItemDetails> uniqueItems = new();
        for (int i = 0; i < _items.Count; i++)
        {
            if (!CheckUniqueItems()) 
                return;

            _items[i].Id = (ItemId) i; 
            EditorUtility.SetDirty(_items[i]);
            AssetDatabase.SaveAssets();
        }

    }

    [ContextMenu("Check IDs")]
    private bool CheckUniqueItems()
    {
        HashSet<ItemDetails> uniqueItems = new();

        foreach (var item in _items)
        {
            if (!uniqueItems.Add(item))
            {
                Debug.LogError($"Error! The item id database has duplicates. Check before running application."); 
                return false;
            }

            Debug.Log($"Item {item.Name} has Id {item.Id.Value}");
        }

        Debug.Log($"All values correct.");

        return true;
    }
#endif
}