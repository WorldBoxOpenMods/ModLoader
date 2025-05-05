using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for actor trait groups
    /// </summary>
    public class ActorTraitGroupBuilder : BaseTraitGroupAssetBuilder<ActorTraitGroupAsset, ActorTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public ActorTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public ActorTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override ActorTraitGroupLibrary GetLibrary()
        {
            return AssetManager.trait_groups;
        }
    }
}
