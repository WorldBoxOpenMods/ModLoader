using NeoModLoader.services;
using NeoModLoader.utils.SerializedAssets;
using Newtonsoft.Json;
namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Class for building assets, you only use this if your mod has custom assets, otherwise use its derived types!!!!!!!!!!!
    /// </summary>
    public class AssetBuilder<A, AL> : Builder where A : Asset, new() where AL : AssetLibrary<A>
    {
        private AssetBuilder() { Library = GetLibrary(); }
        /// <summary>
        /// The Asset being built
        /// </summary>
        public A Asset {  get; protected set; }
        /// <summary>
        /// the Library to add the asset to
        /// </summary>
        public readonly AL Library;
        /// <summary>
        /// Used so the child classes can create their asset before the builder inititates
        /// </summary>
        protected virtual A CreateAsset(string ID) { return new A() { id = ID }; }
        /// <summary>
        /// Initiates the builder
        /// </summary>
        protected virtual void Init() { }
        /// <summary>
        /// Loads the asset from FilePathToBuild
        /// </summary>
        protected virtual void LoadFromPath(string FilePathToBuild)
        {
            SerializableAsset<A> assetSerialized = JsonConvert.DeserializeObject<SerializableAsset<A>>(File.ReadAllText(FilePathToBuild));
            Asset = SerializableAsset<A>.ToAsset(assetSerialized);
        }
        void LoadAssetFromPath(string FilePathToBuild)
        {
            try
            {
                LoadFromPath(FilePathToBuild);
            }
            catch
            {
                LogService.LogError($"the asset {Path.GetFileName(FilePathToBuild)} is outdated or corrupted!, make sure to serialize it on the latest version and use default serialization settings");
            }
        }
        /// <summary>
        /// Creates a builder with a new asset with Id ID, other variables are default
        /// </summary>
        public AssetBuilder(string ID) : this()
        {
            Asset = CreateAsset(ID);
            Init();
        }
        internal string FilePathToBuild = null;
        /// <summary>
        /// Deserializes a Asset loaded from a file path 
        /// </summary>
        /// <remarks>
        /// if LoadImmediatly is false, the asset will be loaded when built
        /// </remarks>
        /// <param name="FilePath">this path starts from the operating system root</param>
        /// <param name="LoadImmediately">the reason for this is when NML automatically loads file assets, it loads the assets before the mod is compiled, and so it then has to deserialize the assets after the mod is compiled because the assets could have delegates which point to the mod, and deserialization will produce an error if the delegates point to nothing</param>
        public AssetBuilder(string FilePath, bool LoadImmediately) : this()
        {
            if (LoadImmediately)
            {
                LoadAssetFromPath(FilePath);
            }
            else
            {
                FilePathToBuild = FilePath;
            }
        }

        /// <summary>
        /// Creates a builder, and the asset being built is copied off a asset with ID CopyFrom
        /// </summary>
        public AssetBuilder(string ID, string CopyFrom) : this()
        {
            bool Cloned = CopyFrom != null;
            if (Cloned)
            {
                Library.clone(out A Asset, Library.get(CopyFrom));
                Asset.id = ID;
                this.Asset = Asset;
            }
            else
            {
                Asset = CreateAsset(ID);
            }
            Init();
        }
        AL GetLibrary() {
            return AssetManager._instance._list.OfType<AL>().FirstOrDefault() ?? throw new NotImplementedException($"No library found for {typeof(A).Name}!");
        }
        /// <summary>
        /// Builds The Asset
        /// </summary>
        public override void Build(bool LinkWithOtherAssets)
        {
            if (FilePathToBuild != null)
            {
               LoadAssetFromPath(FilePathToBuild);
            }
            Library.add(Asset);
            base.Build(LinkWithOtherAssets);
        }
        /// <inheritdoc/>
        public override void LinkAssets() { }
    }
}