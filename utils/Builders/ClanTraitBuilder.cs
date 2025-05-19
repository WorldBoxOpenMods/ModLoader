namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create clan traits
    /// </summary>
    public sealed class ClanTraitBuilder : BaseTraitBuilder<ClanTrait, ClanTraitLibrary>
    {
        /// <inheritdoc/>
        public ClanTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public ClanTraitBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public ClanTraitBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }

        /// <summary>
        /// Stats which are applied to Males in this clan
        /// </summary>
        public BaseStats BaseStatsMale
        {
            get { return Asset.base_stats_male; }
            set { Asset.base_stats_male = value; }
        }
        /// <summary>
        /// Stats which are applied to Females in this clan
        /// </summary>
        public BaseStats BaseStatsFemale
        {
            get { return Asset.base_stats_female; }
            set { Asset.base_stats_female = value; }
        }
    }
}
