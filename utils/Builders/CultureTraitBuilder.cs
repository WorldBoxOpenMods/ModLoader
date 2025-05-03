namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to build culture traits!!
    /// </summary>
    public class CultureTraitBuilder : BaseTraitBuilder<CultureTrait, CultureTraitLibrary>
    {
        /// <inheritdoc/>
        public CultureTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        protected override CultureTraitLibrary GetLibrary()
        {
            return AssetManager.culture_traits;
        }
        /// <inheritdoc/>
        protected override void CreateAsset(string ID)
        {
            Asset = new CultureTrait();
        }
        /// <summary>
        /// Adds a weapon which this culture produces
        /// </summary>
        public void AddWeaponForCulture(string ID)
        {
            Asset.addWeaponSpecial(ID);
        }
        /// <summary>
        /// Adds a subtype of a weapon which the culture produces
        /// </summary>
        public void AddWeaponSubType(string ID)
        {
            Asset.addWeaponSubtype(ID);
        }
        /// <summary>
        /// used for a cultures building layout plan, the zone checker determines weather buildings can be placed in the target zone or not
        /// </summary>
        public void SetTownLayoutPlan(PassableZoneChecker pZoneCheckerDelegate)
        {
            Asset.setTownLayoutPlan(pZoneCheckerDelegate);
        }
    }
}
