extern alias unixsteamwork;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unixsteamwork::Steamworks.Data;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// Creates a achievement Group
    /// </summary>
    public class AchievementGroupBuilder : CategoryAssetBuilder<AchievementGroupAsset, AchievementGroupLibrary>
    {
        /// <inheritdoc/>
        public AchievementGroupBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public AchievementGroupBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override AchievementGroupLibrary GetLibrary()
        {
            return AssetManager.achievement_groups;
        }
    }
}
