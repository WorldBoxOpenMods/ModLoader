namespace NeoModLoader.api.features;

public abstract class ModObjectFeature<TObject> : ModFeature {
  public TObject Object { get; private set; }
  public override bool Init() {
    TObject obj = InitObject();
    if (obj == null) return false;
    Object = obj;
    return true;
  }
  protected abstract TObject InitObject();
  public static implicit operator TObject(ModObjectFeature<TObject> feature) {
    return feature.Object;
  }
}