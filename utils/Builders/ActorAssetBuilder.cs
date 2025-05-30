namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create Actor Assets
    /// NOT FINUSHED!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public sealed class ActorAssetBuilder : UnlockableAssetBuilder<ActorAsset, ActorAssetLibrary>
    {
        /// <inheritdoc/>
        public ActorAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public ActorAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public ActorAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
    }
}
