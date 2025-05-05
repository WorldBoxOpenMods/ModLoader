using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for Item trait groups
    /// </summary>
    public class ItemGroupBuilder : CategoryAssetBuilder<ItemGroupAsset, ItemGroupLibrary>
    {
        /// <inheritdoc/>
        public ItemGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public ItemGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override ItemGroupLibrary GetLibrary()
        {
            return AssetManager.item_groups;
        }
    }
}
