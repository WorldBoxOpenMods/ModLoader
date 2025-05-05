using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// Creates a clan trait group
    /// </summary>
    public class ClanTraitGroupBuilder : BaseTraitGroupAssetBuilder<ClanTraitGroupAsset, ClanTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public ClanTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public ClanTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        protected override ClanTraitGroupLibrary GetLibrary()
        {
            return AssetManager.clan_trait_groups;
        }
    }
}
