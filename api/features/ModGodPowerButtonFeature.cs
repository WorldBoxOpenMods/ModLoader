using NeoModLoader.General;
using UnityEngine;
namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModButtonFeature{TPowersTabFeature}"/> that has the primary purpose to creating a <see cref="PowerButton"/> for using a specific <see cref="GodPower"/>.
/// </summary>
/// <typeparam name="TGodPowerFeature">The <see cref="Type"/> of the feature that's meant to supply a <see cref="GodPower"/> for the <see cref="PowerButton"/> to activate.</typeparam>
/// <typeparam name="TPowersTabFeature">The <see cref="Type"/> of the feature that's meant to supply the <see cref="PowersTab"/> that the <see cref="PowerButton"/> should be added to.</typeparam>
public abstract class ModGodPowerButtonFeature<TGodPowerFeature, TPowersTabFeature> : ModButtonFeature<TPowersTabFeature> where TGodPowerFeature : ModAssetFeature<GodPower> where TPowersTabFeature : ModPowerTabFeature
{
    /// <inheritdoc/>
    public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TGodPowerFeature);
    /// <summary>
    /// A path string to the sprite that the <see cref="PowerButton"/> should use as its icon. This must be a path that can be used by <see cref="Resources.Load{T}(string)"/>.
    /// </summary>
    public abstract string SpritePath { get; }
    /// <summary>
    /// An override of <see cref="ModButtonFeature{TPowersTabFeature}.Init"/> that creates a <see cref="PowerButton"/> for the <see cref="GodPower"/> as a child of the specified <see cref="PowersTab"/>. It uses the ID of the <see cref="GodPower"/> as the button ID, and determines the sprite based on <see cref="SpritePath"/>.
    /// </summary>
    /// <returns>The resulting <see cref="PowerButton"/>.</returns>
    protected override PowerButton InitObject()
    {
        return PowerButtonCreator.CreateGodPowerButton(
            GetFeature<TGodPowerFeature>().Object.id,
            Resources.Load<Sprite>(SpritePath),
            Tab.transform
        );
    }
}