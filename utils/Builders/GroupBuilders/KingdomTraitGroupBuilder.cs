using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for Kingdom trait groups
    /// </summary>
    public class KingdomTraitGroupBuilder : BaseTraitGroupAssetBuilder<KingdomTraitGroupAsset, KingdomTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public KingdomTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public KingdomTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override KingdomTraitGroupLibrary GetLibrary()
        {
            return AssetManager.kingdoms_traits_groups;
        }
    }
}
