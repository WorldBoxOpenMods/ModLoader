using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.General.Game;

/// <summary>
/// This class is used to create item assets to avoid useless coding and avoid necessary code to be forgotten.
/// </summary>
public static class ItemAssetCreator
{
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

    public static ItemAsset CreateMeleeWeapon()
    {
        throw new NotImplementedException();
    }

    public static ItemAsset CreateRangeWeapon()
    {
        throw new NotImplementedException();
    }
    public static ItemAsset CreateArmorOrAccessory()
    {
        throw new NotImplementedException();
    }
}