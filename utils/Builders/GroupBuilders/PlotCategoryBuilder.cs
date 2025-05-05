using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for plot cateogry's
    /// </summary>
    public class PlotCategoryBuilder : CategoryAssetBuilder<PlotCategoryAsset, PlotCategoryLibrary>
    {
        /// <inheritdoc/>
        public PlotCategoryBuilder(string ID) : base(ID)
        {
        }
        /// <inheritdoc/>
        public PlotCategoryBuilder(string ID, string CopyFrom) : base(ID, CopyFrom)
        {
        }
        /// <inheritdoc/>
        protected override PlotCategoryLibrary GetLibrary()
        {
            return AssetManager.plot_category_library;
        }
    }
}
