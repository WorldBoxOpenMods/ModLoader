using NeoModLoader.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    public class ItemModifierBuilder : AugmentationAssetBuilder<ItemAsset, ItemModifierLibrary>
    {
        public ItemModifierBuilder(string ID, int Tier = 0) : base(ID + Tier) { Asset.mod_rank = Tier; Asset.mod_type = ID; }
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
        protected override void CreateAsset(string ID)
        {
            Asset = new ItemAsset()
            {
                id = ID
            };
        }
        /// <summary>
        /// Builds the modifier
        /// </summary>
        public void Build(bool Localize = true)
        {
            LinkWithLibrary();
            if (Localize)
            {
                LM.AddToCurrentLocale(Asset.getLocaleID(), NameID ?? Asset.getLocaleID());
            }
            base.Build();
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
        /// Adds this modifier to a pool of items (like armor, accessory, etc), any item pools with this added can have it added to an item created
        /// </summary>
        public void AddModToItemPool(string PoolID)
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
        /// <summary>
        /// how common this modifier is, in the pools it is in.
        /// </summary>
        public int PoolAmount {  get { return Asset.rarity; } set { Asset.rarity = value; } }
        /// <summary>
        /// The ID of the Name, doesnt have to be set
        /// </summary>
        public string NameID { get { return Asset.translation_key; } set { Asset.translation_key = value; } }
    }
}
