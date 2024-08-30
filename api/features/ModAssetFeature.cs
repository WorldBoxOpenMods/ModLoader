namespace NeoModLoader.api.features;

public abstract class ModAssetFeature<TAsset> : ModObjectFeature<TAsset> where TAsset : Asset {
  protected virtual bool AddToLibrary => true;
  public override bool Init() {
    if (!base.Init()) return false;
    if (AddToLibrary) {
      AssetLibrary<TAsset> library = AssetManager.instance.list.OfType<AssetLibrary<TAsset>>().FirstOrDefault();
      if (library == null) throw new FeatureLoadException($"No library found for {typeof(TAsset).Name}");
      library.add(Object);
    }
    return true;
  }
}