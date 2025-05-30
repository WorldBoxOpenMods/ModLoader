namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A builder for building unlockable assets
    /// </summary>
    public class UnlockableAssetBuilder<A, AL> : AssetBuilder<A, AL> where A : BaseUnlockableAsset, new() where AL : BaseLibraryWithUnlockables<A>
    {
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string ID) : base(ID) { BaseStats = new BaseStats(); }
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// if true, this asset should be discovered by the player to be used
        /// </summary>
        public bool NeedsToBeExplored { set { Asset.needs_to_be_explored = value; } get { return Asset.needs_to_be_explored; } }
        void LinkWithAchievment()
        {
            if (Asset.unlocked_with_achievement)
            {
                Achievement pAchievement = AssetManager.achievements.get(Asset.achievement_id);
                if (pAchievement.unlock_assets == null)
                {
                    pAchievement.unlock_assets = new List<BaseUnlockableAsset>();
                    pAchievement.unlocks_something = true;
                }
                pAchievement.unlock_assets.Add(Asset);
            }
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            LinkWithAchievment();
        }
        /// <summary>
        /// the asset unlocked if this achievment has been unlocked
        /// </summary>
        public string AchievmentToUnlockThis { set { Asset.unlocked_with_achievement = value != null; Asset.achievement_id = value; }
            get { return Asset.achievement_id; }
        }
        /// <summary>
        /// makes the asset available by default
        /// </summary>
        public void UnlockByDefault()
        {
            Asset.unlocked_with_achievement = false;
            Asset.achievement_id = null;
            Asset.needs_to_be_explored = false;
        }
        /// <summary>
        /// the stats of this asset
        /// </summary>
        /// <remarks>
        /// an example would be Stats = new(){ {"health", 2}, {"armor", 2} };
        /// </remarks>
        public Dictionary<string, float> Stats
        {
            set
            {
                foreach (KeyValuePair<string, float> valueTuple in value)
                {
                    BaseStats[valueTuple.Key] = valueTuple.Value;
                }
            }
        }
        /// <summary>
        /// The Stats that are applied to the thing that has this asset, like a actor or a Clan
        /// </summary>
        public BaseStats BaseStats { get { return Asset.base_stats; } set { Asset.base_stats = value; } }
        /// <summary>
        /// the path to the icon, starting from the root directory (GameResources)
        /// </summary>
        public string PathIcon { get { return Asset.path_icon; } set { Asset.path_icon = value;} }
        /// <summary>
        /// if true, the Knowledge Window will display this asset
        /// </summary>
        public bool ShowInKnowledgeWindow { get { return Asset.show_in_knowledge_window; } set { Asset.show_in_knowledge_window = value; } }
    }
}
