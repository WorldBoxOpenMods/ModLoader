using Newtonsoft.Json;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A builder for building unlockable assets
    /// </summary>
    public class UnlockableAssetBuilder<A, AL> : AssetBuilder<A, AL> where A : BaseUnlockableAsset, new() where AL : BaseLibraryWithUnlockables<A>
    {
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public UnlockableAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }

        /// <inheritdoc/>
        protected override void Init(bool Cloned)
        {
            if (!Cloned)
            {
                BaseStats = new BaseStats();
            }
        }

        /// <summary>
        /// if true, this asset should be discovered by the player to be used
        /// </summary>
        public void SetUnlockableSettings(bool ShouldBeDiscoveredByPlayer)
        {
            UnlockByDefault();
            Asset.needs_to_be_explored = ShouldBeDiscoveredByPlayer;
        }
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
        /// Makes the asset unlocked if this achievment has been unlocked
        /// </summary>
        public void SetUnlockableSettings(string AchievmentToUnlockThis)
        {
            UnlockByDefault();
            Asset.unlocked_with_achievement = true;
            Asset.achievement_id = AchievmentToUnlockThis;
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
