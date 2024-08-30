using NeoModLoader.General;
using UnityEngine;
namespace NeoModLoader.api.features;

public abstract class ModGodPowerButtonFeature<TGodPowerFeature, TPowersTabFeature> : ModButtonFeature<TPowersTabFeature> where TGodPowerFeature : ModAssetFeature<GodPower> where TPowersTabFeature : ModPowerTabFeature {
  public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TGodPowerFeature);
  public abstract string SpritePath { get; }
  protected override PowerButton InitObject() {
    return PowerButtonCreator.CreateGodPowerButton(
      GetFeature<TGodPowerFeature>().Object.id,
      Resources.Load<Sprite>(SpritePath),
      Tab.transform
    );
  }
}