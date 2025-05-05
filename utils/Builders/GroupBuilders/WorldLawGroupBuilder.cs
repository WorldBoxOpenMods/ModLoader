using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for WorldLaw groups
    /// </summary>
    public class WorldLawGroupBuilder : CategoryAssetBuilder<WorldLawGroupAsset, WorldLawGroupLibrary>
    {
        /// <inheritdoc/>
        public WorldLawGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public WorldLawGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override WorldLawGroupLibrary GetLibrary()
        {
            return AssetManager.world_law_groups;
        }
    }
}
