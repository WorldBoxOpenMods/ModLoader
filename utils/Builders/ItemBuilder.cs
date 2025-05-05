using HarmonyLib;
using NeoModLoader.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    ///  A Builder to build items
    /// </summary>
    public class ItemBuilder : AugmentationAssetBuilder<ItemAsset, ItemLibrary>
    {
        /// <inheritdoc/>
        public ItemBuilder(string ID, EquipmentType Type) : base(ID, "$"+AssetManager.items.getEquipmentType(Type)) { }
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
        protected override void Init()
        {
            base.Init();
            Asset.setCost(0);
        }
        /// <inheritdoc/>
        protected override void CreateAsset(string ID)
        {
            Asset = new ItemAsset
            {
                id = ID,
                group_id = "sword",
                pool = "equipment",
                name_templates = new List<string>() { "armor_name"}
            };
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
        protected override ItemLibrary GetLibrary()
        {
            return AssetManager.items;
        }
        /// <summary>
        /// Builds the Item, if description is not null it will automatically localize
        /// </summary>
        /// <param name="Description">The Description of the item, if null, localization will have to take place from a localize json file</param>
        public void Build(string Description = null, bool UnlockedByDefault = true)
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
            base.Build();
        }
        void LinkWithLibrary()
        {
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
        /// Adds a name template to this items list of templates, a random template is chosen when the game uses them
        /// </summary>
        public void AddNameTemplate(string TemplateID)
        {
            Asset.name_templates ??= new List<string>();
            Asset.name_templates.Add(TemplateID);
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
        /// The ID of a group of items (subtype) this item is apart of, different cultures have different prefered sub types, cultures only create items apart of their prefered subtypes
        /// </summary>
        public string SubType { get { return Asset.equipment_subtype; } set { Asset.equipment_subtype = value; } }
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
        public void SetResource1(string ID, int Num, int Minimum = 0)
        {
            Asset.cost_resource_1 = Num;
            Asset.minimum_city_storage_resource_1 = Minimum;
            Asset.cost_resource_id_1 = ID;
        }
        /// <summary>
        /// The ID of the secound material and its amount a city needs to craft this item
        /// </summary>
        public void SetResource2(string ID, int Num)
        {
            Asset.cost_resource_2 = Num;
            Asset.cost_resource_id_2 = ID;
        }
        /// <summary>
        /// the durability of the item, by default its 100
        /// </summary>
        public int Durability { get { return Asset.durability; } set { Asset.durability = value; } }
        /// <summary>
        /// if true, it must also have a SubType
        /// </summary>
        public bool CanBeCraftedByCities { get { return Asset.is_pool_weapon; } set { Asset.is_pool_weapon = value; } }
        /// <summary>
        /// Adds a modifier to this item's list, when it is created all of them are applied
        /// </summary>
        /// <param name="ModifierID"></param>
        public void AddItemModifier(string ModifierID)
        {
            Asset.item_modifier_ids.AddItem(ModifierID);
        }
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
    }
}