using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.General.Game;

/// <summary>
/// This class is used to create item assets to avoid useless coding and avoid necessary code to be forgotten.
/// </summary>
public static class ItemAssetCreator
{
    /// <summary>
    /// Create material for weapon
    /// </summary>
    /// <remarks>You should add it to <see cref="AssetManager.items_material_weapon"/> manually</remarks>
    /// <param name="id"></param>
    /// <param name="base_stats"></param>
    /// <param name="cost_gold"></param>
    /// <param name="cost_resources"></param>
    /// <param name="equipment_value"></param>
    /// <param name="metallic"></param>
    /// <param name="minimum_city_storage_resource_1"></param>
    /// <param name="mod_rank"></param>
    /// <param name="quality"></param>
    /// <param name="tech_needed"></param>
    /// <returns></returns>
    public static ItemAsset CreateWeaponMaterial(
        string id,
        BaseStats base_stats = null,
        int cost_gold = 0,
        KeyValuePair<string, int>[] cost_resources = null,
        int equipment_value = 0,
        bool metallic = false,
        int minimum_city_storage_resource_1 = 0,
        int mod_rank = 0,
        ItemQuality quality = ItemQuality.Normal,
        string tech_needed = null
    )
    {
        ItemAsset asset = CreateAccessoryOrArmorMaterial(id, base_stats, cost_gold, cost_resources, equipment_value,
            minimum_city_storage_resource_1, mod_rank, quality, tech_needed);
        asset.metallic = metallic;
        return asset;
    }
    /// <summary>
    /// Create material for accessory or armor.
    /// </summary>
    /// <remarks>You should add it to <see cref="AssetManager.items_material_accessory"/> or <see cref="AssetManager.items_material_armor"/> manually</remarks>
    /// <param name="id"></param>
    /// <param name="base_stats"></param>
    /// <param name="cost_gold"></param>
    /// <param name="cost_resources"></param>
    /// <param name="equipment_value"></param>
    /// <param name="minimum_city_storage_resource_1"></param>
    /// <param name="mod_rank"></param>
    /// <param name="quality"></param>
    /// <param name="tech_needed"></param>
    /// <returns></returns>
    public static ItemAsset CreateAccessoryOrArmorMaterial(
        string id,
        BaseStats base_stats = null,
        int cost_gold = 0,
        KeyValuePair<string, int>[] cost_resources = null,
        int equipment_value = 0,
        int minimum_city_storage_resource_1 = 0,
        int mod_rank = 0,
        ItemQuality quality = ItemQuality.Normal,
        string tech_needed = null
    )
    {
        ItemAsset asset = new ItemAsset();
        asset.id = id;
        asset.base_stats = base_stats ?? asset.base_stats;
        asset.cost_gold = cost_gold;
        asset.equipment_value = equipment_value;
        asset.minimum_city_storage_resource_1 = minimum_city_storage_resource_1;
        asset.mod_rank = mod_rank;
        asset.quality = quality;
        asset.tech_needed = tech_needed;

        asset.cost_resource_id_1 = "none";
        asset.cost_resource_id_2 = "none";
        if (cost_resources != null)
        {
            switch (cost_resources.Length)
            {
                case 0:
                    break;
                case 1:
                    asset.cost_resource_1 = cost_resources[0].Value;
                    asset.cost_resource_id_1 = cost_resources[0].Key;
                    break;
                case >= 2:
                    asset.cost_resource_1 = cost_resources[0].Value;
                    asset.cost_resource_id_1 = cost_resources[0].Key;
                    asset.cost_resource_2 = cost_resources[1].Value;
                    asset.cost_resource_id_2 = cost_resources[1].Key;
                    break;
            }
        }
        return asset;
    }
    /// <summary>
    /// Create and add an item modifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mod_type"></param>
    /// <param name="mod_rank"></param>
    /// <param name="translation_key"></param>
    /// <param name="pools"></param>
    /// <param name="rarity"></param>
    /// <param name="equipment_value"></param>
    /// <param name="quality"></param>
    /// <param name="base_stats"></param>
    /// <param name="action_attack_target"></param>
    /// <param name="action_special_effect"></param>
    /// <param name="special_effect_interval"></param>
    /// <returns></returns>
    public static ItemAsset CreateAndAddModifier(
        string id,
        string mod_type,
        int mod_rank,
        string translation_key,
        string[] pools,
        int rarity = 1,
        int equipment_value = 0,
        ItemQuality quality = ItemQuality.Normal,
        BaseStats base_stats = null,
        AttackAction action_attack_target = null,
        WorldAction action_special_effect = null,
        float special_effect_interval = 0.1f
    )
    {
        ItemAsset asset = new ItemAsset();
        asset.id = id;
        asset.mod_type = mod_type;
        asset.mod_rank = mod_rank;
        asset.translation_key = translation_key;
        asset.rarity = Math.Min(100, rarity);
        asset.equipment_value = equipment_value;
        asset.quality = quality;
        asset.base_stats = base_stats;
        asset.action_attack_target = action_attack_target;
        asset.action_special_effect = action_special_effect;
        asset.special_effect_interval = special_effect_interval;

        StringBuilder pool_builder = new StringBuilder();
        foreach (string pool in pools)
        {
            pool_builder.Append(pool);
            pool_builder.Append(',');
        }
        pool_builder.Remove(pool_builder.Length - 1, 1);
        asset.pool = pool_builder.ToString();

        foreach (string pool_id in pools)
        {
            if (!AssetManager.items_modifiers.pools.ContainsKey(pool_id))
            {
                LogService.LogWarning($"Invalid pool id {pool_id} for modifier {id}");
                continue;
            }

            for (int i = 0; i < rarity; i++)
            {
                AssetManager.items_modifiers.pools[pool_id].Add(asset);
            }
        }

        AssetManager.items_modifiers.add(asset);
        
        return asset;
    }
    /// <summary>
    /// Create and add a melee weapon
    /// </summary>
    /// <param name="id"></param>
    /// <param name="base_stats"></param>
    /// <param name="materials"></param>
    /// <param name="item_modifiers"></param>
    /// <param name="name_class"></param>
    /// <param name="name_templates"></param>
    /// <param name="tech_needed"></param>
    /// <param name="action_attack_target"></param>
    /// <param name="action_special_effect"></param>
    /// <param name="special_effect_interval"></param>
    /// <param name="equipment_value"></param>
    /// <param name="path_slash_animation"></param>
    /// <returns></returns>
    public static ItemAsset CreateMeleeWeapon(
        string id,
        BaseStats base_stats = null,
        List<string> materials = null,
        List<string> item_modifiers = null,
        string name_class = null,
        List<string> name_templates = null,
        string tech_needed = null,
        AttackAction action_attack_target = null,
        WorldAction action_special_effect = null,
        float special_effect_interval = 1f,
        int equipment_value = 0,
        string path_slash_animation = "effects/slashes/slash_base"
        )
    {
        ItemAsset asset = AssetManager.items.clone(id, "_melee");
        
        asset.base_stats = base_stats ?? asset.base_stats;
        asset.materials = materials ?? asset.materials;
        asset.item_modifiers = item_modifiers ?? asset.item_modifiers;
        asset.name_class = string.IsNullOrEmpty(name_class) ? asset.name_class : name_class;
        asset.name_templates = name_templates ?? asset.name_templates;
        asset.tech_needed = tech_needed;
        asset.action_attack_target = action_attack_target;
        asset.action_special_effect = action_special_effect;
        asset.special_effect_interval = special_effect_interval;
        asset.equipment_value = equipment_value;
        asset.path_slash_animation = path_slash_animation;

        asset.attackType = WeaponType.Melee;
        asset.equipmentType = EquipmentType.Weapon;
        return asset;
    }
    /// <summary>
    /// Create and add a range weapon
    /// </summary>
    /// <param name="id"></param>
    /// <param name="projectile"></param>
    /// <param name="base_stats"></param>
    /// <param name="materials"></param>
    /// <param name="item_modifiers"></param>
    /// <param name="name_class"></param>
    /// <param name="name_templates"></param>
    /// <param name="tech_needed"></param>
    /// <param name="action_attack_target"></param>
    /// <param name="action_special_effect"></param>
    /// <param name="special_effect_interval"></param>
    /// <param name="equipment_value"></param>
    /// <param name="path_slash_animation"></param>
    /// <returns></returns>
    public static ItemAsset CreateRangeWeapon(
        string id,
        string projectile,
        BaseStats base_stats = null,
        List<string> materials = null,
        List<string> item_modifiers = null,
        string name_class = null,
        List<string> name_templates = null,
        string tech_needed = null,
        AttackAction action_attack_target = null,
        WorldAction action_special_effect = null,
        float special_effect_interval = 1f,
        int equipment_value = 0,
        string path_slash_animation = "effects/slashes/slash_punch"
        )
    {
        ItemAsset asset = AssetManager.items.clone(id, "_range");
        
        asset.base_stats = base_stats ?? asset.base_stats;
        asset.materials = materials ?? asset.materials;
        asset.item_modifiers = item_modifiers ?? asset.item_modifiers;
        asset.name_class = string.IsNullOrEmpty(name_class) ? asset.name_class : name_class;
        asset.name_templates = name_templates ?? asset.name_templates;
        asset.tech_needed = tech_needed;
        asset.action_attack_target = action_attack_target;
        asset.action_special_effect = action_special_effect;
        asset.special_effect_interval = special_effect_interval;
        asset.equipment_value = equipment_value;
        asset.path_slash_animation = path_slash_animation;
        asset.projectile = string.IsNullOrEmpty(projectile) ? "snowball" : projectile;
        
        StringBuilder warning_builder = new StringBuilder();
        warning_builder.AppendLine($"Some unexpected for {id} as a range weapon:");
        if (string.IsNullOrEmpty(projectile))
        {
            warning_builder.AppendLine("\t projectile is null or empty. ");
        }

        asset.attackType = WeaponType.Range;
        asset.equipmentType = EquipmentType.Weapon;
        
        return asset;
    }
    /// <summary>
    /// Create and add an armor or accessory item.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="equipmentType"></param>
    /// <param name="base_stats"></param>
    /// <param name="materials"></param>
    /// <param name="item_modifiers"></param>
    /// <param name="name_class"></param>
    /// <param name="name_templates"></param>
    /// <param name="tech_needed"></param>
    /// <param name="action_attack_target"></param>
    /// <param name="action_special_effect"></param>
    /// <param name="special_effect_interval"></param>
    /// <param name="equipment_value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static ItemAsset CreateArmorOrAccessory(
        string id,
        EquipmentType equipmentType,
        BaseStats base_stats = null,
        List<string> materials = null,
        List<string> item_modifiers = null,
        string name_class = null,
        List<string> name_templates = null,
        string tech_needed = null,
        AttackAction action_attack_target = null,
        WorldAction action_special_effect = null,
        float special_effect_interval = 1f,
        int equipment_value = 0
    )
    {
        string template = equipmentType switch
        {
            EquipmentType.Armor => "armor",
            EquipmentType.Boots => "boots",
            EquipmentType.Helmet => "helmet",
            EquipmentType.Ring => "ring",
            EquipmentType.Amulet => "amulet",
            _ => throw new ArgumentOutOfRangeException(nameof(equipmentType), equipmentType, null)
        };
        ItemAsset asset = AssetManager.items.clone(id, template);
        
        asset.base_stats = base_stats ?? asset.base_stats;
        asset.materials = materials ?? asset.materials;
        asset.item_modifiers = item_modifiers ?? asset.item_modifiers;
        asset.name_class = string.IsNullOrEmpty(name_class) ? asset.name_class : name_class;
        asset.name_templates = name_templates ?? asset.name_templates;
        asset.tech_needed = tech_needed;
        asset.action_attack_target = action_attack_target;
        asset.action_special_effect = action_special_effect;
        asset.special_effect_interval = special_effect_interval;
        asset.equipment_value = equipment_value;

        asset.equipmentType = equipmentType;
        return asset;
    }
}