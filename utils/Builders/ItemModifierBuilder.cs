using NeoModLoader.General;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder which creates item modifiers!
    /// </summary>
    /*public sealed class ItemModifierBuilder : AugmentationAssetBuilder<ItemAsset, ItemModifierLibrary> // same issue as item assets @melvin @melvin @melvin :3
    {
        /// <inheritdoc/>
        public ItemModifierBuilder(string FilePath, bool LoadImmediately) :base(FilePath, LoadImmediately) { }
        /// <summary>
        /// A Modifier Builder
        /// </summary>
        public ItemModifierBuilder(string ID, int Tier = 1) : base(ID + Tier, ID+1) { Asset.mod_rank = Tier; Asset.mod_type = ID; }
        void LinkWithLibrary()
        {
            for (int i = 0; i < Asset.rarity; i++)
            {
                if (Asset.pool.Contains("weapon"))
                {
                    Library.pools["weapon"].Add(Asset);
                }
                if (Asset.pool.Contains("armor"))
                {
                    Library.pools["armor"].Add(Asset);
                }
                if (Asset.pool.Contains("accessory"))
                {
                    Library.pools["accessory"].Add(Asset);
                }
            }
        }
        /// <inheritdoc/>
        public override void Build(bool LinkWithOtherAssets)
        {
           Build(true, LinkWithOtherAssets);
        }
        /// <summary>
        /// Builds the modifier
        /// </summary>
        public void Build(bool Localize = true, bool LinkWithOtherAssets = false)
        {
            LinkWithLibrary();
            if (Localize)
            {
                LM.AddToCurrentLocale(Asset.getLocaleID(), Asset.getLocaleID());
            }
            base.Build(LinkWithOtherAssets);
        }
        /// <summary>
        /// The Displayed Rarity of the Asset
        /// </summary>
        public Rarity Rarity { get { return Asset.quality; } set { Asset.quality = value; } }
        /// <summary>
        /// if true, the player can give this modifier to items
        /// </summary>
        public bool CanModifiersBeGiven { get { return Asset.mod_can_be_given; } set { Asset.mod_can_be_given = value; } }
        /// <summary>
        /// the pools of items (like armor, accessory, etc) this mod is in, any item pools with this added can have it added to an item created
        /// </summary>
        public IEnumerable<string> ItemPools
        {
            set
            {
                foreach (string PoolID in value)
                {
                    if (Asset.pool.Length > 0)
                    {
                        Asset.pool += "," + PoolID;
                    }
                    else
                    {
                        Asset.pool = PoolID;
                    }
                }
            }
            get
            {
                return Asset.pool.Split(',');
            }
        }
        /// <summary>
        /// how common this modifier is, in the pools it is in.
        /// </summary>
        public int PoolAmount {  get { return Asset.rarity; } set { Asset.rarity = value; } }
        /// <summary>
        /// The ID of the Name, doesnt have to be set
        /// </summary>
        public string NameID { get { return Asset.translation_key; } set { Asset.translation_key = value; } }
    }*/
}
