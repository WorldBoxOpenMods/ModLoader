using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for Language trait groups
    /// </summary>
    public class LanguageTraitGroupBuilder : BaseTraitGroupAssetBuilder<LanguageTraitGroupAsset, LanguageTraitGroupLibrary>
    {
        /// <inheritdoc/>
        public LanguageTraitGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public LanguageTraitGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override LanguageTraitGroupLibrary GetLibrary()
        {
            return AssetManager.language_trait_groups;
        }
    }
}
