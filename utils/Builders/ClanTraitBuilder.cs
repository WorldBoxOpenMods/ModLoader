using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create clan traits
    /// </summary>
    public class ClanTraitBuilder : BaseTraitBuilder<ClanTrait, ClanTraitLibrary>
    {
        /// <inheritdoc/>
        public ClanTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        protected override ClanTraitLibrary GetLibrary()
        {
            return AssetManager.clan_traits;
        }
        /// <inheritdoc/>
        protected override void CreateAsset(string ID)
        {
            Asset = new ClanTrait();
        }
        /// <summary>
        /// Stats which are applied to Males in this clan
        /// </summary>
        public BaseStats BaseStatsMale
        {
            get { return Asset.base_stats_male; }
            set { Asset.base_stats_male = value; }
        }
        /// <summary>
        /// Stats which are applied to FeMales in this clan
        /// </summary>
        public BaseStats BaseStatsFemale
        {
            get { return Asset.base_stats_female; }
            set { Asset.base_stats_female = value; }
        }
    }
}
