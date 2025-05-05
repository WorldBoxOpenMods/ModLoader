using NeoModLoader.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create category assets
    /// </summary>
    public class CategoryAssetBuilder<A, AL> : AssetBuilder<A, AL> where A : BaseCategoryAsset where AL : AssetLibrary<A>
    {
        /// <inheritdoc/>
        public CategoryAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public CategoryAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// Builds The Category Asset
        /// </summary>
        public void Build(bool AutoLocalize)
        {
            if (AutoLocalize)
            {
                Localize();
            }
            base.Build();
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
        /// Sets the color using a Color32
        /// </summary>
        public void SetColor(Color32 color)
        {
            ColorHexCode = Toolbox.colorToHex(color);
        }
    }
}
