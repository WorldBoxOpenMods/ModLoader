namespace NeoModLoader.utils.SerializedAssets
{
    /// <summary>
    /// a serializable item asset, including culture traits which have it and its type
    /// </summary>
    public class SerializedItemAsset : SerializableAsset<ItemAsset>
    {
        internal string[] CultureTraitsThisItemIsIn;
        internal string[] CultureTraitsThisItemsTypeIsIn;
        /// <summary>
        /// Converts the item asset to a serializable version
        /// </summary>
        public static SerializedItemAsset FromAsset(ItemAsset Asset, IEnumerable<string> cultureTraitsItem = null, IEnumerable<string> cultureTraitsType = null)
        {
            SerializedItemAsset asset = new();
            Serialize(Asset, asset);
            if (cultureTraitsItem != null)
            {
                asset.CultureTraitsThisItemIsIn = cultureTraitsItem.ToArray();
            }
            if (cultureTraitsType != null)
            {
                asset.CultureTraitsThisItemsTypeIsIn = cultureTraitsType.ToArray();
            }
            return asset;
        }
        /// <summary>
        /// Converts the serializable version to a actor trait asset
        /// </summary>
        public static ItemAsset ToAsset(SerializedItemAsset Asset)
        {
            ItemAsset asset = new();
            Deserialize(Asset, asset);
            return asset;
        }
    }
}