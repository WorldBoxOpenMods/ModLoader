using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for Culture trait groups
    /// </summary>
    public class CultureTraitGroupBuilder : BaseTraitGroupAssetBuilder<CultureTraitGroupAsset, CultureTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public CultureTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public CultureTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override CultureTraitGroupLibrary GetLibrary()
        {
            return AssetManager.culture_trait_groups;
        }
    }
}
