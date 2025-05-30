namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Class For All Builders
    /// </summary>
    public abstract class Builder
    {
        /// <summary>
        /// Builds Something
        /// </summary>
        public virtual void Build(bool LinkWithOtherAssets) { if(LinkWithOtherAssets) { LinkAssets(); } }
        /// <summary>
        /// links this builder with other assets
        /// </summary>
        public abstract void LinkAssets();
    }
}