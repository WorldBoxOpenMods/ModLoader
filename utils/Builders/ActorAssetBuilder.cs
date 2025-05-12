using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder to create Actor Assets
    /// NOT FINUSHED!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public class ActorAssetBuilder : UnlockableAssetBuilder<ActorAsset, ActorAssetLibrary>
    {
        /// <inheritdoc/>
        public ActorAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public ActorAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public ActorAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
    }
}
