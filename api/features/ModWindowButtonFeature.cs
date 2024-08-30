using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
namespace NeoModLoader.api.features;

public abstract class ModWindowButtonFeature<TWindowFeature, TPowersTabFeature> : ModButtonFeature<TPowersTabFeature> where TWindowFeature : ModObjectFeature<ScrollWindow> where TPowersTabFeature : PowerTabFeature {
  public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TWindowFeature);
  protected ScrollWindow Window => GetFeature<TWindowFeature>();
  public abstract UnityAction WindowOpenAction { get; }
  public abstract string SpritePath { get; }
  protected override PowerButton InitObject() {
    return PowerButtonCreator.CreateSimpleButton(
      Window.name,
      WindowOpenAction,
      Resources.Load<Sprite>(SpritePath),
      Tab.transform
    );
  }
}