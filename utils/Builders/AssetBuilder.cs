using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Class for building assets, you only use this if your mod has custom assets, otherwise use its derived types!!!!!!!!!!!
    /// </summary>
    public class AssetBuilder<A, AL> : Builder where A : Asset where AL : AssetLibrary<A>
    {
        /// <summary>
        /// The Asset being built
        /// </summary>
        public A Asset;
        /// <summary>
        /// the Library to add the asset to
        /// </summary>
        public readonly AL Library;
        /// <summary>
        /// Used so the child classes can create their asset before the builder inititates
        /// </summary>
        protected virtual void CreateAsset(string ID)
        {

        }
        /// <summary>
        /// Initiates the builder
        /// </summary>
        protected virtual void Init()
        {

        }
        /// <summary>
        /// A Tool to help you create Assets!
        /// </summary>
        public AssetBuilder(string ID)
        {
            Library = GetLibrary();
            CreateAsset(ID);
            Init();
        }

        /// <summary>
        /// Creates a builder, and the asset being built is copied off a asset with ID CopyFrom
        /// </summary>
        public AssetBuilder(string ID, string CopyFrom)
        {
            Library = GetLibrary();
            Library.clone(out Asset, Library.get(CopyFrom));
            Asset.id = ID;
            Init();
        }
        /// <summary>
        /// Must return the AssetLibrary this builder uses
        /// </summary>
        protected virtual AL GetLibrary() {
            return null;
        }
        /// <summary>
        /// Builds The Asset
        /// </summary>
        public override void Build()
        {
            Library.add(Asset);
        }
    }
}
