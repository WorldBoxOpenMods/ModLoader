namespace NeoModLoader.api.features;

public abstract class ModButtonFeature<TPowersTabFeature> : ModObjectFeature<PowerButton> where TPowersTabFeature : ModPowerTabFeature {
  public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TPowersTabFeature);
  protected PowersTab Tab => GetFeature<TPowersTabFeature>();
  public override bool Init() {
    return base.Init() && GetFeature<TPowersTabFeature>().PositionButton(Object);
  }
}