using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for Subspecies trait groups
    /// </summary>
    public class SubspeciesTraitGroupBuilder : BaseTraitGroupAssetBuilder<SubspeciesTraitGroupAsset, SubspeciesTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public SubspeciesTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public SubspeciesTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override SubspeciesTraitGroupLibrary GetLibrary()
        {
            return AssetManager.subspecies_trait_groups;
        }
    }
}
