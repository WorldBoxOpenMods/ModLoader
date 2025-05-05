namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// ??????????????????? ¯\_(ツ)_/¯
    /// </summary>
    public class BaseTraitGroupAssetBuilder<A, AL> : CategoryAssetBuilder<A, AL> where A : BaseTraitGroupAsset where AL : BaseCategoryLibrary<A>
    {
        /// <inheritdoc/>
        public BaseTraitGroupAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public BaseTraitGroupAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
    }
}
