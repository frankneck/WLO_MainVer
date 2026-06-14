using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ItemsDataAuthoring : MonoBehaviour
{
    [SerializeField] private ItemScriptableDatabase ItemDatabase;

    class Baker : Baker<ItemsDataAuthoring>
    {
        public override void Bake(ItemsDataAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // read list from database
            int count = authoring.ItemDatabase.Items.Count;
            var dataBaseList = authoring.ItemDatabase.Items;

            // creating blob array
            var builder = new BlobBuilder(Allocator.Temp);
            ref ItemImmutableData root = ref builder.ConstructRoot<ItemImmutableData>();
            var items = builder.Allocate(ref root.ItemDataArray, count);

            // filling by values 
            // add more info
            for (int i = 0; i < count; i++)
            {
                items[i] = new ItemData
                {
                    MaxStack = dataBaseList[i].MaxStack,
                    Type = dataBaseList[i].Type,
                    AllowedSlots = dataBaseList[i].AllowedSlots
                };
            }

            var blobReference = builder.CreateBlobAssetReference<ItemImmutableData>(Allocator.Persistent);
            builder.Dispose();

            AddBlobAsset(ref blobReference, out var hash);
            AddComponent(entity, new ItemDataBlobArray
            {
                Value = blobReference
            });
        }
    }
}