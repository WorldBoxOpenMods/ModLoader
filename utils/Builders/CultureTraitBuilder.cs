namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to build culture traits
    /// </summary>
    public sealed class CultureTraitBuilder : BaseTraitBuilder<CultureTrait, CultureTraitLibrary>
    {
        /// <inheritdoc/>
        public CultureTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public CultureTraitBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public CultureTraitBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// the weapons which this culture produces
        /// </summary>
        public IEnumerable<string> Weapons { get { return Asset.related_weapons_ids; } set { foreach (string weapon in value) { Asset.addWeaponSpecial(weapon); } } }
        /// <summary>
        /// the weapon sub types which this culture produces
        /// </summary>
        public IEnumerable<string> WeaponSubTypes { get { return Asset.related_weapon_subtype_ids; } set { foreach (string weapon in value) { Asset.addWeaponSubtype(weapon); } } }
        /// <summary>
        /// used for a cultures building layout plan, the zone checker determines weather buildings can be placed in the target zone or not
        /// </summary>
        public PassableZoneChecker TownLayoutPlan
        {
            get { return Asset.passable_zone_checker; }
            set
            {
                Asset.setTownLayoutPlan(value);
            }
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            if (Asset.town_layout_plan)
            {
                OpposeAllOtherTraits = new[] { (CultureTrait trait) => trait.town_layout_plan };
            }
            base.LinkAssets();
        }
    }
}