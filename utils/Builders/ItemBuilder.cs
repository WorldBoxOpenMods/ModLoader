using NeoModLoader.General;
using NeoModLoader.utils.SerializedAssets;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    ///  A Builder to build items
    /// </summary>
    /*public sealed class ItemBuilder : AugmentationAssetBuilder<ItemAsset, ItemLibrary> // congrats melvin, your ItemBuilder broke on a fundamental level within 1 singular update of existing somehow, I am under severe time pressure rn and cannot be bothered to look into the cause so have fun whenever you see this Ig
    {
        static string GetEquipmentType(EquipmentType pType) => pType switch
        {
            EquipmentType.Weapon => "$melee",
            EquipmentType.Helmet => "$helmet",
            EquipmentType.Armor => "$armor",
            EquipmentType.Boots => "$boots",
            EquipmentType.Ring => "$ring",
            EquipmentType.Amulet => "$amulet",
            _ => "$equipment"
        };
        /// <inheritdoc/>
        public ItemBuilder(string Path, bool LoadImmediately) : base(Path, LoadImmediately) { }
        /// <inheritdoc/>
        public ItemBuilder(string ID, EquipmentType Type) : base(ID, GetEquipmentType(Type)) { }
        /// <summary>
        /// creates a weapon, and if projectileifranged is not null, a ranged weapon with projectileifranged being the ID of the projectile
        /// </summary>
        public ItemBuilder(string WeaponID, string ProjectileIfRanged = null) : base(WeaponID, "$melee") {
            if (ProjectileIfRanged != null)
            {
                ConvertIntoRangedWeapon(ProjectileIfRanged);
            }
        }
        /// <inheritdoc/>
        protected override void LoadFromPath(string FilePathToBuild)
        {
            SerializedItemAsset assetSerialized = JsonConvert.DeserializeObject<SerializedItemAsset>(File.ReadAllText(FilePathToBuild));
            Asset = SerializedItemAsset.ToAsset(assetSerialized);
            CultureTraitsThisWeaponIsIn = assetSerialized.CultureTraitsThisItemIsIn;
            CultureTraitsThisWeaponsTypeIsIn = assetSerialized.CultureTraitsThisItemsTypeIsIn;
        }
        /// <inheritdoc/>
        protected override ItemAsset CreateAsset(string ID)
        {
            ItemAsset asset =  new()
            {
                id = ID,
                group_id = "sword",
                pool = "equipment",
                name_templates = new List<string>() { "armor_name"}
            };
            asset.setCost(0);
            return asset;
        }
        /// <summary>
        /// converts the Asset into a ranged weapon, its type must be Weapon to do this
        /// </summary>
        public void ConvertIntoRangedWeapon(string ProjectileID)
        {
            Asset.pool = "range";
            Asset.attack_type = WeaponType.Range;
            Asset.projectile = ProjectileID;
            Asset.base_stats["projectiles"] = 1f;
            Asset.base_stats["damage_range"] = 0.6f;
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            foreach(string CultureTraitID in CultureTraitsThisWeaponIsIn)
            {
                AssetManager.culture_traits.get(CultureTraitID).addWeaponSpecial(Asset.id);
            }
            foreach (string CultureTraitID in CultureTraitsThisWeaponsTypeIsIn)
            {
                AssetManager.culture_traits.get(CultureTraitID).addWeaponSubtype(WeaponSubType);
            }
            if (Asset.item_modifier_ids != null)
            {
                Asset.item_modifiers = new ItemAsset[Asset.item_modifier_ids.Length];
                for (int i = 0; i < Asset.item_modifier_ids.Length; i++)
                {
                    string tModID = Asset.item_modifier_ids[i];
                    ItemAsset tModData = AssetManager.items_modifiers.get(tModID);
                    if (tModData == null)
                    {
                        BaseAssetLibrary.logAssetError("ItemLibrary: Item Modifier Asset <e>not found</e>", tModID);
                    }
                    else
                    {
                        Asset.item_modifiers[i] = tModData;
                    }
                }
            }
            base.LinkAssets();
        }
        /// <inheritdoc/>
        public override void Build(bool LinkWithOtherAssets)
        {
            Build(null, true, LinkWithOtherAssets);
        }
        /// <summary>
        /// Builds the Item, if description is not null it will automatically localize
        /// </summary>
        /// <param name="Description">The Description of the item, if null, localization will have to take place from a localize json file</param>
        /// <param name="UnlockedByDefault">is unlocked by default</param>
        /// <param name="LinkWithOtherAssets">links with other assets</param>
        public void Build(string Description = null, bool UnlockedByDefault = true, bool LinkWithOtherAssets = false)
        {
            if(Description != null)
            {
                Localize(Asset.getLocaleID(), Description);
            }
            AddWeaponsSprite();
            LinkWithLibrary();
            if (UnlockedByDefault)
            {
                UnlockByDefault();
            }
            base.Build(LinkWithOtherAssets);
        }
        void LinkWithLibrary()
        {
            if (!Library.equipment_by_subtypes.ContainsKey(Asset.equipment_subtype))
            {
                Library.equipment_by_subtypes.Add(Asset.equipment_subtype, new List<ItemAsset>());
            }
            Library.equipment_by_subtypes[Asset.equipment_subtype].Add(Asset);
            if (Asset.is_pool_weapon)
            {
                Library.pool_weapon_assets_all.Add(Asset);
            }
            if (!Asset.is_pool_weapon)
            {
                string tGroupId = Asset.group_id;
                if (!Library.equipment_by_groups_all.ContainsKey(tGroupId))
                {
                    Library.equipment_by_groups_all.Add(tGroupId, new List<ItemAsset>());
                }
                Library.equipment_by_groups_all[tGroupId].Add(Asset);
            }
            if (Asset.isUnlocked())
            {
                if (Asset.is_pool_weapon && !Library.pool_weapon_assets_unlocked.Contains(Asset))
                {
                    Library.pool_weapon_assets_unlocked.Add(Asset);
                }
                if (!Asset.is_pool_weapon)
                {
                    string tGroupId = Asset.group_id;
                    if (!Library.equipment_by_groups_unlocked.ContainsKey(tGroupId))
                    {
                        Library.equipment_by_groups_unlocked.Add(tGroupId, new List<ItemAsset>());
                    }
                    List<ItemAsset> tList = Library.equipment_by_groups_unlocked[tGroupId];
                    if (!tList.Contains(Asset))
                    {
                        tList.Add(Asset);
                    }
                }
            }
        }
        /// <summary>
        /// the name templates of this item, a random template is chosen when the game uses them
        /// </summary>
        public IEnumerable<string> NameTemplates
        {
            get { return Asset.name_templates; }
            set { Asset.name_templates = value.ToList(); }
        }
        void AddWeaponsSprite()
        {
            var dictItems = ActorAnimationLoader._dict_items;
            var sprite = Resources.Load<Sprite>("weapons/" + Asset.id);
            dictItems.Add("w_" + Asset.id, new List<Sprite>() { sprite });
        }
        /// <summary>
        /// Localizes the Items name and description the current language
        /// </summary>
        public void Localize(string Name, string Description)
        {
            LM.AddToCurrentLocale(Asset.getLocaleID(), Name);
            LM.AddToCurrentLocale(Asset.getDescriptionID(), Description);
        }
        /// <summary>
        /// The ID of the Name, doesnt have to be set
        /// </summary>
        public string NameID { get { return Asset.translation_key; } set { Asset.translation_key = value; } }
        /// <summary>
        /// The Displayed Rarity of the Asset
        /// </summary>
        public Rarity Rarity { get { return Asset.quality; } set { Asset.quality = value; } }
        /// <summary>
        /// The Value of this equipment, used for when cities want to craft something, they prefer equipment with higher values
        /// </summary>
        public int EquipmentValue { get { return Asset.equipment_value; } set { Asset.equipment_value = value; } }
        /// <summary>
        /// the texture path to the animation that gets played when a actor holding this weapon attacks something
        /// </summary>
        public string SlashAnimationPath { get { return Asset.path_slash_animation; } set { Asset.path_slash_animation = value;} }
        /// <summary>
        /// if true, the weapon displayed in the actors HAND is animated
        /// </summary>
        public bool Animated { get { return Asset.animated; } set { Asset.animated = value; } }
        /// <summary>
        /// The ID of a group of items (subtype) this item is apart of, different cultures have different prefered sub types, cultures only create weapons apart of their prefered subtypes
        /// </summary>
        public string WeaponSubType { get { return Asset.equipment_subtype; } set { Asset.equipment_subtype = value; } }
        /// <summary>
        /// culture traits who have this items subtype added, MUST be a weapon
        /// </summary>
        /// <remarks>
        /// the builder must link its assets so the cultures actually add it
        /// </remarks>
        public IEnumerable<string> CultureTraitsThisWeaponsTypeIsIn;
        /// <summary>
        /// the cultures who have this item in their preferred weapons, it must be a weapon
        /// </summary>
        /// <remarks>
        /// the builder must link its assets so the cultures actually add it
        /// </remarks>
        public IEnumerable<string> CultureTraitsThisWeaponIsIn;
        /// <summary>
        /// the amount of coins a city has to spend to craft this item
        /// </summary>
        public int CoinCost { get { return Asset.cost_coins_resources; } set { Asset.cost_coins_resources = value; } }
        /// <summary>
        /// the amount of coins a city has to spend to craft/repair this time
        /// </summary>
        public int GoldCost { get { return Asset.cost_gold; } set { Asset.cost_gold = value; } }
        /// <summary>
        /// The ID of the first material and its amount a city needs to craft this item, and also the Minimum amount required
        /// </summary>
        public ValueTuple<string, int, int> Resource1 { get { return new(Asset.cost_resource_id_1, Asset.cost_resource_1, Asset.minimum_city_storage_resource_1); } set { Asset.cost_resource_1 = value.Item2; Asset.minimum_city_storage_resource_1 = value.Item3; Asset.cost_resource_id_1 = value.Item1; } }
        /// <summary>
        /// The ID of the secound material and its amount a city needs to craft this item
        /// </summary>
        public ValueTuple<string, int> Resource2 { get { return new(Asset.cost_resource_id_2, Asset.cost_resource_2); } set { Asset.cost_resource_id_2 = value.Item1; Asset.cost_resource_2 = value.Item2; } }
        /// <summary>
        /// the durability of the item, by default its 100
        /// </summary>
        public int Durability { get { return Asset.durability; } set { Asset.durability = value; } }
        /// <summary>
        /// if true, it must also have a SubType
        /// </summary>
        public bool CanBeCraftedByCities { get { return Asset.is_pool_weapon; } set { Asset.is_pool_weapon = value; } }
        /// <summary>
        /// the modifiers in this item's list, when it is created all of them are applied
        /// </summary>
        public IEnumerable<string> ItemModifiers { get { return Asset.item_modifier_ids; } set { Asset.item_modifier_ids = value.ToArray(); } }
        /// <summary>
        /// the material of the item, doesnt change any properties, just for displaying
        /// </summary>
        public string Material { get { return Asset.material; } set { Asset.material = value; } }
        /// <summary>
        /// only used for SFX, if a weapon is metallic and a actor hits someone with it, it produces noise
        /// </summary>
        public bool Metallic { get { return Asset.metallic; } set { Asset.metallic = value; } }
        /// <summary>
        /// not used by the game at the moment!
        /// </summary>
        public int Rate { get { return Asset.pool_rate; } set { Asset.pool_rate = value; } }
    }*/
}