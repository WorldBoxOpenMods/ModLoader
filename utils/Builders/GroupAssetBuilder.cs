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
    /// A Builder to create group/category assets
    /// </summary>
    public class GroupAssetBuilder<A> : AssetBuilder<A, AssetLibrary<A>> where A : BaseCategoryAsset, new()
    {
        /// <inheritdoc/>
        public GroupAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public GroupAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// Builds The Group Asset
        /// </summary>
        public void Build(bool AutoLocalize)
        {
            if (AutoLocalize)
            {
                Localize();
            }
            base.Build();
        }
        /// <inheritdoc/>
        protected override void CreateAsset(string ID)
        {
            Asset = new A
            {
                id = ID
            };
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
