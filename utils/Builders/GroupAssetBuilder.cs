using NeoModLoader.General;
using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create group/category assets
    /// </summary>
    public sealed class GroupAssetBuilder<A> : AssetBuilder<A, AssetLibrary<A>> where A : BaseCategoryAsset, new()
    {
        /// <inheritdoc/>
        public GroupAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public GroupAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public GroupAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// builds it, if autolocalize is true it will localize
        /// </summary>
        public new void Build(bool AutoLocalize = true)
        {
            if (AutoLocalize)
            {
                Localize();
            }
            base.Build(AutoLocalize);
        }
        /// <summary>
        /// Localizes the name
        /// </summary>
        public void Localize(string LocalName = null)
        {
            LocalName ??= Asset.getLocaleID();
            LM.AddToCurrentLocale(Asset.getLocaleID(), LocalName);
        }
        /// <summary>
        /// The ID used for localization
        /// </summary>
        public string Name { get { return Asset.name; } set { Asset.name = value; } }
        /// <summary>
        /// A Hex Value that represents the color
        /// </summary>
        public string ColorHexCode { get { return Asset.color; } set { Asset.color = value; } }
        /// <summary>
        /// Sets the color using a Color
        /// </summary>
        public void SetColor(Color color)
        {
            ColorHexCode = Toolbox.colorToHex(color);
        }
    }
}