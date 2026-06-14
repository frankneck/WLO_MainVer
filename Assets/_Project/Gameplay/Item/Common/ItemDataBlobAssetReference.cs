using Unity.Entities;

/// <summary>
/// Reference to Blob Asset
/// </summary>
public struct ItemDataBlobArray : IComponentData
{
    public BlobAssetReference<ItemImmutableData> Value;
}