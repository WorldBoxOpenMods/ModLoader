using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;
#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using generic = Il2CppSystem.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#else
using generic = System.Collections.Generic;
using System.Collections.Generic;
#endif
namespace NeoModLoader.utils
{
    public delegate void Linker(Asset Asset);
    /// <summary>
    /// General purpose asset linker which links your assets and can load them as Files
    /// </summary>
    public class AssetLinker
    {
        public generic.List<Asset> Assets = new generic.List<Asset>();
        generic.Dictionary<Type, Linker> CustomLinkers = new generic.Dictionary<Type, Linker>();
        internal generic.List<string> AssetFilePaths = new generic.List<string>();
        /// <summary>
        /// gets the library of this asset
        /// </summary>
        public static void AddToLibrary(Asset Asset)
        {
            foreach(BaseAssetLibrary library in AssetManager._instance._list)
            {
                var type = library.GetType();
                if (type?.GetField("t", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType == Asset.GetType())
                {
                    type.GetMethod("add").Invoke(library, new object[] { Asset });
                    return;
                }
            }
            throw new Exception($"could not get the library of asset {Asset.GetType().Name}!");
        }
        /// <summary>
        /// gets an asset from its Name
        /// </summary>
        /// <remarks>not the ID of the asset, the Name of its type</remarks>
        public static Type GetAssetFromName(string Name)
        {
            return Type.GetType(Name + ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }
        /// <summary>
        /// Adds an asset to the linker to be linked
        /// </summary>
        /// <returns></returns>
        public Asset AddAsset(Asset Asset)
        {
            Assets.Add(Asset);
            return Asset;
        }
        /// <summary>
        /// Adds Assets to their Libraries
        /// </summary>
        public void AddAssets(bool Link = true)
        {
            foreach (string FilePath in AssetFilePaths)
            {
                try
                {
                    Assets.Add(LoadFile(FilePath));
                }
                catch (Exception e)
                {
                    LogService.LogError($"Could not load Asset of type {Path.GetExtension(FilePath)}!");
                }
            }
            foreach (Asset asset in Assets)
            {
               AddToLibrary(asset);
            }
            if (Link)
            {
                LinkAssets();
            }
        }
        /// <summary>
        /// adds a custom linker function for a asset. can be a custom or native asset
        /// </summary>
        public void AddCustomLinker(Type Type, Linker Linker)
        {
            if(CustomLinkers.ContainsKey(Type)){
                LogService.LogError($"Asset of Type {Type} already has linker!");
            }
            CustomLinkers.Add(Type, Linker);
        }
        /// <summary>
        /// links an asset with other assets, along with post-init modifications
        /// </summary>
        /// <remarks>not all assets have a linker, to add your own linker, call the AddLinker Function</remarks>
        public void LinkAsset(Asset asset)
        {
            string type = asset.GetType().Name;
            MethodInfo Function = typeof(AssetLinker).GetMethod("Link" + type, BindingFlags.Static | BindingFlags.Public);
            if (Function == null)
            {
                if(!CustomLinkers.TryGetValue(asset.GetType(), out Linker linker))
                {
                    LogService.LogWarning($"there is no asset linker for asset type {type}!");
                    return;
                }
                Function = linker.Method;
            }
            Function.Invoke(null, new object[] { asset });
        }
        /// <summary>
        /// Links the Assets
        /// </summary>
        public void LinkAssets()
        {
            foreach (Asset asset in Assets)
            {
                LinkAsset(asset);
            }
        }
        /// <summary>
        /// Loads a asset from a file
        /// </summary>
        public static Asset LoadFile(string FilePath)
        {
            SerializableAsset assetSerialized = JsonConvert.DeserializeObject<SerializableAsset>(File.ReadAllText(FilePath));
            return SerializableAsset.ToAsset(assetSerialized, GetAssetFromName(Path.GetExtension(FilePath)));
        }
        /// <summary>
        /// saves a asset to a folder
        /// </summary>
        public static void SaveFile(string FilePath, Asset Asset)
        {
            SerializableAsset assetSerialized = SerializableAsset.FromAsset(Asset);
            File.WriteAllText(FilePath + "/" + Asset.id + "." + Asset.GetType().Name, JsonConvert.SerializeObject(assetSerialized));
        }
        #region Linkers
        public static void GenerateZombie(ActorAsset Asset)
        {
            ActorAssetLibrary Library = AssetManager.actor_library;
            if (!Asset.isTemplateAsset() && Asset.zombie_auto_asset && Asset.can_turn_into_zombie)
            {
                string tZombieID = Asset.getZombieID();
                ActorAsset tGeneratedZombieAsset = Library.clone(tZombieID, Asset.id);
                ActorAsset ZombieAsset = tGeneratedZombieAsset;
                Library.setDefaultZombieFields(Library.t, Asset, Asset.default_animal);
                ActorTextureSubAsset tSubTexture = new ActorTextureSubAsset(Asset.texture_path_zombie_for_auto_loader_main, Library.t.has_advanced_textures);
                Library.t.texture_asset = tSubTexture;
                ActorTextureSubAsset tOriginalSubTexture = Asset.texture_asset;
                tSubTexture.shadow = tOriginalSubTexture.shadow;
                tSubTexture.shadow_texture = tOriginalSubTexture.shadow_texture;
                tSubTexture.shadow_texture_egg = tOriginalSubTexture.shadow_texture_egg;
                tSubTexture.shadow_texture_baby = tOriginalSubTexture.shadow_texture_baby;
                if (Library.hasSpriteInResources(Asset.texture_path_zombie_for_auto_loader_main))
                {
                    tSubTexture.texture_path_main = Asset.texture_path_zombie_for_auto_loader_main;
                    tSubTexture.texture_heads = Asset.texture_path_zombie_for_auto_loader_heads;
                }
                else
                {
                    tSubTexture.texture_path_main = Asset.texture_asset.texture_path_main;
                    tSubTexture.texture_heads = Asset.texture_asset.texture_heads;
                    Library.t.dynamic_sprite_zombie = true;
                }
                if (Asset.animation_swim == null)
                {
                    Library.t.animation_swim = null;
                }
                Library.loadTexturesAndSprites(ZombieAsset);
            }

        }
        public static void LinkWithAchievment(BaseUnlockableAsset Asset)
        {
            if (Asset.unlocked_with_achievement)
            {
                Achievement pAchievement = AssetManager.achievements.get(Asset.achievement_id);
                if (pAchievement.unlock_assets == null)
                {
                    pAchievement.unlock_assets = new generic.List<BaseUnlockableAsset>();
                    pAchievement.unlocks_something = true;
                }
                pAchievement.unlock_assets.Add(Asset);
            }
        }
        public static void LinkActorAsset(Asset asset)
        {
            ActorAsset Asset = asset as ActorAsset;
            ActorAssetLibrary Library = AssetManager.actor_library;
            Library.loadTexturesAndSprites(Asset);
            GenerateZombie(Asset);
            if (Asset.shadow)
            {
                Asset.texture_asset.loadShadow();
            }
            if (!Asset.is_boat)
            {
                Asset.generateFmodPaths(Asset.id);
            }
            if (Asset.action_dead_animation != null)
            {
                Asset.special_dead_animation = true;
            }
            if (!string.IsNullOrEmpty(Asset.base_asset_id))
            {
                ActorAsset tBaseAsset = Library.get(Asset.base_asset_id);
                Asset.units = tBaseAsset.units;
            }
            if (Asset.is_humanoid && !Asset.unit_zombie)
            {
                Library._humanoids_amount++;
            }
            if (Asset.avatar_prefab != string.Empty)
            {
                Asset.has_avatar_prefab = true;
            }
            if (Asset.get_override_sprite != null)
            {
                Asset.has_override_sprite = true;
            }
            if (Asset.get_override_avatar_frames != null)
            {
                Asset.has_override_avatar_frames = true;
            }
            if (!string.IsNullOrEmpty(Asset.architecture_id))
            {
                Asset.architecture_asset = AssetManager.architecture_library.get(Asset.architecture_id);
            }
            if (Asset.spell_ids != null && Asset.spell_ids.Count != 0)
            {
                Asset.spells = new SpellHolder();
                Asset.spells.mergeWith(Asset.spell_ids);
            }
            if (Asset.is_boat)
            {
                Library.list_only_boat_assets.Add(Asset);
            }
            if (Asset.color_hex != null)
            {
                #if !IL2CPP
                Asset.color = new Color32?(Toolbox.makeColor(Asset.color_hex));
                #endif
            }
            if (Asset.check_flip == null)
            {
                Asset.check_flip = IL2CPPHelper.Convert<WorldAction>((BaseSimObject _, WorldTile _) => true);
            }
            LinkWithAchievment(Asset);
        }
        public static void LinkArchitectureAsset(Asset asset)
        {
            ArchitectureAsset Asset = asset as ArchitectureAsset;
            if (!string.IsNullOrEmpty(Asset.spread_biome_id))
            {
                Asset.spread_biome = true;
            }
            if (!Asset.isTemplateAsset())
            {
                AssetManager.architecture_library.loadAutoBuildingsForAsset(Asset);
                foreach (var tSharedBuilding in Asset.shared_building_orders)
                {
                    Asset.addBuildingOrderKey(tSharedBuilding.Item1, tSharedBuilding.Item2);
                }
            }
            if (!Asset.isTemplateAsset() && Asset.generate_buildings)
            {
                string id = Asset.id;
                foreach (string text in Asset.styled_building_orders)
                {
                    string text2 = Asset.building_ids_for_construction[text];
                    string generation_target = Asset.generation_target;
                    BuildingAsset building = AssetManager.architecture_library.get(generation_target).getBuilding(text);
                    BuildingAsset buildingAsset = AssetManager.buildings.clone(text2, building.id);
                    buildingAsset.group = "civ_building";
                    buildingAsset.mini_civ_auto_load = true;
                    buildingAsset.civ_kingdom = id;
                    buildingAsset.main_path = "buildings/civ_main/" + id + "/";
                    buildingAsset.can_be_upgraded = false;
                    buildingAsset.has_sprite_construction = true;
                    if (Asset.spread_biome)
                    {
                        buildingAsset.spread_biome = true;
                        buildingAsset.spread_biome_id = Asset.spread_biome_id;
                    }
                    buildingAsset.material = Asset.material;
                    if (buildingAsset.material == "jelly")
                    {
                        buildingAsset.setAtlasID("buildings_wobbly", "buildings");
                    }
                    buildingAsset.shadow = Asset.has_shadows;
                    buildingAsset.burnable = Asset.burnable_buildings;
                    buildingAsset.affected_by_acid = Asset.acid_affected_buildings;
                    if (text == "order_library")
                    {
                        buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
                    }
                    else if (text == "order_temple")
                    {
                        buildingAsset.fundament = new BuildingFundament(2, 2, 3, 0);
                    }

                    else if (text == "order_hall_0")
                    {
                        buildingAsset.fundament = new BuildingFundament(3, 3, 4, 0);
                    }

                    else if (text == "order_tent")
                    {
                        buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
                    }

                    else if (!(text == "order_house_0"))
                    {
                    }
                    if (!(text == "order_barracks"))
                    {
                    }
                    else if (text == "order_windmill_0")
                    {
                        buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
                        if (buildingAsset.shadow)
                        {
                            buildingAsset.setShadow(0.4f, 0.38f, 0.47f);
                        }
                    }

                    if (text == "order_watch_tower")
                    {
                        buildingAsset.fundament = new BuildingFundament(1, 1, 1, 0);
                    }
                    else if (text == "order_docks_0")
                    {
                        string text3 = "docks_" + id;
                        buildingAsset.upgrade_to = text3;
                        buildingAsset.can_be_upgraded = true;
                    }

                    else if (text == "order_docks_1")
                    {
                        string text4 = "fishing_docks_" + id;
                        buildingAsset.upgraded_from = text4;
                        buildingAsset.has_sprites_main_disabled = false;
                    }
                }
            }
        }
        public static void LinkPhenotype(Asset asset)
        {
            PhenotypeAsset Asset = asset as PhenotypeAsset;
            PhenotypeLibrary Library = AssetManager.phenotype_library;
            Library.createShades(Asset);
            Asset.phenotype_index = Library.list.Count - 1;
            Library._phenotypes_assets_by_index.Add(Asset.phenotype_index, Asset);   
        }
        public static void LinkItem(Asset asset)
        {
            EquipmentAsset Asset = asset as EquipmentAsset;
            if (Asset.item_modifier_ids != null)
            {
                Asset.item_modifiers = new ItemModAsset[Asset.item_modifier_ids.Length];
                for (int i = 0; i < Asset.item_modifier_ids.Length; i++)
                {
                    string tModID = Asset.item_modifier_ids[i];
                    ItemModAsset tModData = AssetManager.items_modifiers.get(tModID);
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
            if (Asset.is_pool_weapon)
            {
                Asset.path_gameplay_sprite = "items/weapons/w_" + Asset.id;
            }
            if (string.IsNullOrEmpty(Asset.path_icon))
            {
                Asset.path_icon = "ui/Icons/items/icon_" + Asset.id;
                int tResourcesGoldCostResources = 0;
                if (Asset.cost_resource_id_1 != "none")
                {
                    ResourceAsset tResource = AssetManager.resources.get(Asset.cost_resource_id_1);
                    tResourcesGoldCostResources += tResource.money_cost;
                }
                if (Asset.cost_resource_id_2 != "none")
                {
                    ResourceAsset tResource2 = AssetManager.resources.get(Asset.cost_resource_id_2);
                    tResourcesGoldCostResources += tResource2.money_cost;
                }
                Asset.cost_coins_resources = tResourcesGoldCostResources;
            }
            if (Asset.is_pool_weapon)
            {
                Asset.gameplay_sprites = SpriteTextureLoader.getSpriteList(Asset.path_gameplay_sprite, false);
                if (Asset.gameplay_sprites.Length == 0)
                {
                    Debug.LogError("Weapon Texture is Missing: " + Asset.path_gameplay_sprite);
                }
            }
            LinkAugmentationAsset(Asset);
        }
        public static void LinkItemModifier(Asset asset)
        {
            ItemModAsset Asset = asset as ItemModAsset;
            ItemModifierLibrary Library = AssetManager.items_modifiers;
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
            LinkAugmentationAsset(Asset);
        }
        public static void LinkAugmentationAsset(BaseAugmentationAsset Asset)
        {
            if (Asset.decision_ids != null)
            {
                Asset.decisions_assets = new DecisionAsset[Asset.decision_ids.Count];
                for (int i = 0; i < Asset.decision_ids.Count; i++)
                {
                    string tDecisionID = Asset.decision_ids[i];
                    DecisionAsset tDecisionAsset = AssetManager.decisions_library.get(tDecisionID);
                    Asset.decisions_assets[i] = tDecisionAsset;
                }
            }
            Asset.linkSpells();
            Asset.linkCombatActions();
            LinkWithAchievment(Asset);
        }
        public static void LinkSubspeciesTrait(Asset asset)
        {
            SubspeciesTrait Asset = asset as SubspeciesTrait;
            SubspeciesTraitLibrary Library = AssetManager.subspecies_traits;
            if (Asset.id_phenotype != null)
            {
                PhenotypeAsset Phenotype = AssetManager.phenotype_library.get(Asset.id_phenotype);
                Phenotype.subspecies_trait_id = Asset.id;
                Asset.priority = Phenotype.priority;
            }
            if (Asset.is_mutation_skin)
            {
                OpposeOtherTraits(Asset, (SubspeciesTrait trait) => trait.is_mutation_skin, AssetManager.subspecies_traits);
            }
            if (Asset.phenotype_skin)
            {
                OpposeOtherTraits(Asset, (SubspeciesTrait trait) => trait.phenotype_skin, AssetManager.subspecies_traits);
            }
            if (Asset.phenotype_egg)
            {
                OpposeOtherTraits(Asset, (SubspeciesTrait trait) => trait.phenotype_egg, AssetManager.subspecies_traits);
            }
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.Add(Asset);
            }
            if (Asset.in_mutation_pot_add)
            {
                int tRate = Asset.rarity.GetRate();
                Library._pot_mutation_traits_add.AddTimes(tRate, Asset);
            }
            if (Asset.in_mutation_pot_remove)
            {
                int tRate2 = Asset.rarity.GetRate();
                Library._pot_mutation_traits_remove.AddTimes(tRate2, Asset);
            }
            if (Asset.phenotype_egg && Asset.after_hatch_from_egg_action != null)
            {
                Asset.has_after_hatch_from_egg_action = true;
            }
            Library.loadSpritesPaths(Asset);
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.Add(Asset);
            }
            if (Asset.in_mutation_pot_add)
            {
                int tRate = Asset.rarity.GetRate();
                Library._pot_mutation_traits_add.AddTimes(tRate, Asset);
            }
            if (Asset.in_mutation_pot_remove)
            {
                int tRate2 = Asset.rarity.GetRate();
                Library._pot_mutation_traits_remove.AddTimes(tRate2, Asset);
            }
            if (Asset.phenotype_egg && Asset.after_hatch_from_egg_action != null)
            {
                Asset.has_after_hatch_from_egg_action = true;
            }
            LinkBaseTrait(Asset, Library);
        }
        public static void OpposeOtherTraits<A>(A Trait, Func<A, bool> Oppose, BaseTraitLibrary<A> Library) where A : BaseTrait<A> 
        {
            foreach (A asset in Library.list)
            {
                if (asset.id != Trait.id && Oppose(asset))
                {
                    Trait.addOpposite(asset.id);
                }
            }
        }
        public static void LinkBaseTrait<A>(A Asset, BaseTraitLibrary<A> Library) where A : BaseTrait<A>
        {
            foreach (ActorAsset tActorAsset in AssetManager.actor_library.list)
            {
                generic.List<string> traits = Library.getDefaultTraitsForMeta(tActorAsset);
                if (traits != null && traits.Contains(Asset.id))
                {
                    Asset.default_for_actor_assets ??= new generic.List<ActorAsset>();
                    Asset.default_for_actor_assets.Add(tActorAsset);
                }
            }
            if (Asset.opposite_list != null && Asset.opposite_list.Count > 0)
            {
                Asset.opposite_traits = new generic.HashSet<A>(Asset.opposite_list.Count);
                foreach (string tID in Asset.opposite_list)
                {
                    A tOppositeTrait = Library.get(tID);
                    Asset.opposite_traits.Add(tOppositeTrait);
                }
            }
            if (Asset.traits_to_remove_ids != null)
            {
                int tCount = Asset.traits_to_remove_ids.Length;
                Asset.traits_to_remove = new A[tCount].Convert();
             
                for (int i = 0; i < tCount; i++)
                {
                    string ID = Asset.traits_to_remove_ids[i];
                    A tTraitToAdd = Library.get(ID);
                    Asset.traits_to_remove[i] = tTraitToAdd;
                }
            }
            if (string.IsNullOrEmpty(Asset.path_icon))
            {
                Asset.path_icon = Library.icon_path + Asset.getLocaleID();
            }
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.AddTimes(Asset.spawn_random_rate, Asset);
            }
            LinkAugmentationAsset(Asset);
        }
        public static void LinkActorTrait(Asset asset)
        {
            ActorTrait Asset = asset as ActorTrait;
            ActorTraitLibrary Library = AssetManager.traits;
            if (Asset.in_training_dummy_combat_pot)
            {
                Library.pot_traits_combat.Add(Asset);
            }
            if (Asset.is_mutation_box_allowed)
            {
                Library.pot_traits_mutation_box.Add(Asset);
            }
            if (Asset.rate_acquire_grow_up != 0)
            {
                for (int j = 0; j < Asset.rate_acquire_grow_up; j++)
                {
                    Library.pot_traits_growup.Add(Asset);
                }
            }
            if (Asset.rate_birth != 0)
            {
                for (int i = 0; i < Asset.rate_birth; i++)
                {
                    Library.pot_traits_birth.Add(Asset);
                }
            }
            Library.checkDefault(Asset);
            Asset.only_active_on_era_flag = Asset.era_active_moon || Asset.era_active_night;
            LinkBaseTrait(Asset, Library);
        }
        public static void LinkClanTrait(Asset asset)
        {
            LinkBaseTrait<ClanTrait>(asset as ClanTrait, AssetManager.clan_traits);
        }
        public static void LinkCultureTrait(Asset asset)
        {
            CultureTrait Asset = asset as CultureTrait;
            if (Asset.town_layout_plan)
            {
                OpposeOtherTraits(Asset, (CultureTrait trait) => trait.town_layout_plan, AssetManager.culture_traits);
            }
            LinkBaseTrait(Asset, AssetManager.culture_traits);
        }
        public static void LinkKingdomAsset(Asset assett)
        {
            KingdomAsset Asset = assett as KingdomAsset;
            KingdomLibrary Library = AssetManager.kingdoms;
            Asset.addTag("everyone");
            if (Asset.friendship_for_everyone && !Asset.brain)
            {
                foreach (KingdomAsset asset in Library.list)
                {
                    asset.addFriendlyTag(Asset.id);
                }
            }
            if (Asset.default_kingdom_color != null)
            {
                if (string.Equals(Asset.default_kingdom_color.id, "ASSET_ID"))
                {
                    Asset.default_kingdom_color.id = "kingdom_library_color_" + Asset.id;
                }
            }
            else
            {
                Asset.default_kingdom_color = Library._shared_default_color;
            }
        }
        public static void LinkBuildingAsset(Asset asset)
        {
            BuildingAsset Asset = asset as BuildingAsset;
            if (Asset.step_action != null)
            {
                Asset.has_step_action = true;
            }
            if (Asset.get_map_icon_color != null)
            {
                Asset.has_get_map_icon_color = true;
            }
            BuildingAsset buildingAsset = Asset;
            generic.HashSet<BiomeTag> biome_tags_growth = Asset.biome_tags_growth;
            buildingAsset.has_biome_tags = biome_tags_growth != null && biome_tags_growth.Count > 0;
            BuildingAsset buildingAsset2 = Asset;
            generic.HashSet<BiomeTag> biome_tags_spread = Asset.biome_tags_spread;
            buildingAsset2.has_biome_tags_spread = biome_tags_spread != null && biome_tags_spread.Count > 0;
        }
        public static void LinkBiomeAsset(Asset asset)
        {
            AssetManager.biome_library.addBiomeToPool(asset as BiomeAsset);
        }
        public static void LinkSpellAsset(Asset asset)
        {
            SpellAsset spellAsset = asset as SpellAsset;
            if (spellAsset.decision_ids != null)
            {
                spellAsset.decisions_assets = new DecisionAsset[spellAsset.decision_ids.Count];
                for (int i = 0; i < spellAsset.decision_ids.Count; i++)
                {
                    string text = spellAsset.decision_ids[i];
                    DecisionAsset decisionAsset = AssetManager.decisions_library.get(text);
                    spellAsset.decisions_assets[i] = decisionAsset;
                }
            }
        }
        public static void LinkStatusAsset(Asset asset)
        {
            StatusAsset statusAsset = asset as StatusAsset;
            if (statusAsset.get_override_sprite != null)
            {
                statusAsset.has_override_sprite = true;
                statusAsset.need_visual_render = true;
            }
            if (statusAsset.get_override_sprite_position != null)
            {
                statusAsset.has_override_sprite_position = true;
            }
            if (statusAsset.get_override_sprite_rotation_z != null)
            {
                statusAsset.has_override_sprite_rotation_z = true;
            }
            if (statusAsset.texture != null)
            {
                statusAsset.need_visual_render = true;
            }
            if (statusAsset.texture != null && statusAsset.sprite_list == null)
            {
                statusAsset.sprite_list = SpriteTextureLoader.getSpriteList("effects/" + statusAsset.texture, false);
            }
        }
        public static void LinkHotkeyAsset(Asset asset)
        {
            HotkeyAsset hotkeyAsset = asset as HotkeyAsset;
            hotkeyAsset.overridden_key_1 = hotkeyAsset.default_key_1;
            hotkeyAsset.overridden_key_2 = hotkeyAsset.default_key_2;
            hotkeyAsset.overridden_key_3 = hotkeyAsset.default_key_3;
            hotkeyAsset.overridden_key_mod_1 = hotkeyAsset.default_key_mod_1;
            hotkeyAsset.overridden_key_mod_2 = hotkeyAsset.default_key_mod_2;
            hotkeyAsset.overridden_key_mod_3 = hotkeyAsset.default_key_mod_3;
            
            if (hotkeyAsset.default_key_mod_1 != null)
            {
                HotkeyLibrary.mod_keys = HotkeyLibrary.mod_keys.Convert().AddToArray(hotkeyAsset.default_key_mod_1);
            }
            if (hotkeyAsset.default_key_mod_2 != null)
            {
                HotkeyLibrary.mod_keys = HotkeyLibrary.mod_keys.Convert().AddToArray(hotkeyAsset.default_key_mod_2);
            }
            if (hotkeyAsset.default_key_mod_3 != null)
            {
                HotkeyLibrary.mod_keys = HotkeyLibrary.mod_keys.Convert().AddToArray(hotkeyAsset.default_key_mod_3);
            }
            if (hotkeyAsset.just_pressed_action != null)
            {
                AssetManager.hotkey_library.action_hotkeys = AssetManager.hotkey_library.action_hotkeys.Convert().AddToArray(hotkeyAsset);
            }
            else if (hotkeyAsset.holding_action != null)
            {
                AssetManager.hotkey_library.action_hotkeys = AssetManager.hotkey_library.action_hotkeys.Convert().AddToArray(hotkeyAsset);
            }
            
        }
        public static void LinkWorldBehaviourAsset(Asset asset)
        {
            WorldBehaviourAsset worldBehaviourAsset = asset as WorldBehaviourAsset;
            worldBehaviourAsset.manager = new WorldBehaviour(worldBehaviourAsset);
        }
        #endregion
    }
}