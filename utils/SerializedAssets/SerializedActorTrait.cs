using NeoModLoader.utils.Builders;

namespace NeoModLoader.utils.SerializedAssets
{
    /// <summary>
    /// a serializable actor trait, including its custom additional base stats method
    /// </summary>
    public class SerializedActorTrait : SerializableAsset<ActorTrait>
    {
        /// <summary>
        /// the class and the method of the additionalbasestatsmethod
        /// </summary>
        public string AdditionalBaseStatsMethod;
        /// <summary>
        /// Converts the actor trait asset to a serializable version
        /// </summary>
        public static SerializedActorTrait FromAsset(ActorTrait Asset, GetAdditionalBaseStatsMethod Method = null)
        {
            SerializedActorTrait asset = new SerializedActorTrait();
            Serialize(Asset, asset);
            if (Method != null)
            {
                asset.AdditionalBaseStatsMethod = Method.AsString(false);
            }
            return asset;
        }
        /// <summary>
        /// Converts the serializable version to a actor trait asset
        /// </summary>
        public static ActorTrait ToAsset(SerializedActorTrait Asset)
        {
            ActorTrait asset = new();
            Deserialize(Asset, asset);
            if (Asset.AdditionalBaseStatsMethod != null)
            {
                ActorTraitBuilder.AdditionalBaseStatMethods.TryAdd(asset.id, Asset.AdditionalBaseStatsMethod.AsDelegate<GetAdditionalBaseStatsMethod>());
            }
            return asset;
        }
    }
}
