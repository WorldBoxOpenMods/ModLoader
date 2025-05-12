using NeoModLoader.services;
using Newtonsoft.Json;
using UnityEngine;
namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Class for building assets, you only use this if your mod has custom assets, otherwise use its derived types!!!!!!!!!!!
    /// </summary>
    public class AssetBuilder<A, AL> : Builder where A : Asset, new() where AL : AssetLibrary<A>
    {
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
        protected virtual void Init(bool Cloned) { }
        /// <summary>
        /// Loads the asset from a file path
        /// </summary>
        protected virtual void LoadFromPath()
        {
            SerializableAsset<A> assetSerialized = JsonConvert.DeserializeObject<SerializableAsset<A>>(File.ReadAllText(FilePathToBuild));
            Asset = SerializableAsset<A>.ToAsset(assetSerialized);
        }
        /// <summary>
        /// Creates a builder with a new asset with Id ID, other variables are default
        /// </summary>
        public AssetBuilder(string ID)
        {
            Library = GetLibrary();
            Asset = CreateAsset(ID);
            Init(false);
        }
        internal string FilePathToBuild = null;
        /// <summary>
        /// Deserializes a Asset loaded from a file path
        /// </summary>
        /// <remarks>
        /// if LoadImmediatly is false, make sure to build this!
        /// </remarks>
        public AssetBuilder(string FilePath, bool LoadImmediately)
        {
            Library = GetLibrary();
            if (LoadImmediately)
            {
                Asset = JsonConvert.DeserializeObject<A>(File.ReadAllText(FilePath));
            }
            else
            {
                FilePathToBuild = FilePath;
            }
        }

        /// <summary>
        /// Creates a builder, and the asset being built is copied off a asset with ID CopyFrom
        /// </summary>
        public AssetBuilder(string ID, string CopyFrom)
        {
            Library = GetLibrary();
            Library.clone(out A Asset, Library.get(CopyFrom));
            Asset.id = ID;
            this.Asset = Asset;
            Init(true);
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
                Debug.Log(FilePathToBuild);
              //  try
                {
                    LoadFromPath();
                }
               /* catch
                {
                    LogService.LogError($"the asset {Path.GetFileName(FilePathToBuild)} is outdated or corrupted!, make sure to serialize it on the latest version and use default serialization settings");
                }*/
            }
            Library.add(Asset);
            base.Build(LinkWithOtherAssets);
        }
        /// <inheritdoc/>
        public override void LinkAssets() { }
    }
}
