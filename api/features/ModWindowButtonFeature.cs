using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModButtonFeature{TPowersTabFeature}"/> that has the primary purpose to creating a <see cref="PowerButton"/> for opening a specific <see cref="ScrollWindow"/>.
/// </summary>
/// <typeparam name="TWindowFeature">The <see cref="Type"/> of the feature that's meant to supply a <see cref="ScrollWindow"/> for the <see cref="PowerButton"/> to activate.</typeparam>
/// <typeparam name="TPowersTabFeature">The <see cref="Type"/> of the feature that's meant to supply the <see cref="PowersTab"/> that the <see cref="PowerButton"/> should be added to.</typeparam>
public abstract class ModWindowButtonFeature<TWindowFeature, TPowersTabFeature> : ModButtonFeature<TPowersTabFeature> where TWindowFeature : ModObjectFeature<ScrollWindow> where TPowersTabFeature : ModPowerTabFeature
{
    /// <inheritdoc/>
    public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TWindowFeature);
    /// <summary>
    /// A shorthand for grabbing the <see cref="ScrollWindow"/> that the <see cref="PowerButton"/> should open.
    /// </summary>
    protected ScrollWindow Window => GetFeature<TWindowFeature>();
    /// <summary>
    /// A <see cref="UnityAction"/> that the <see cref="PowerButton"/> should perform when clicked. This should be set to prepare and open the <see cref="ScrollWindow"/>.
    /// </summary>
    public abstract UnityAction WindowOpenAction { get; }
    /// <summary>
    /// A path string to the sprite that the <see cref="PowerButton"/> should use as its icon. This must be a path that can be used by <see cref="Resources.Load{T}(string)"/>.
    /// </summary>
    public abstract string SpritePath { get; }
    /// <summary>
    /// An override of <see cref="ModButtonFeature{TPowersTabFeature}.Init"/> that creates a <see cref="PowerButton"/> for the <see cref="ScrollWindow"/> as a child of the specified <see cref="PowersTab"/>. It uses the name of the <see cref="ScrollWindow"/> as the button ID, and determines the sprite based on <see cref="SpritePath"/>. The <see cref="PowerButton"/> will perform <see cref="WindowOpenAction"/> when clicked.
    /// </summary>
    /// <returns>The resulting <see cref="PowerButton"/>.</returns>
    protected override PowerButton InitObject()
    {
        return PowerButtonCreator.CreateSimpleButton(
            Window.name,
            WindowOpenAction,
            Resources.Load<Sprite>(SpritePath),
            Tab.transform
        );
    }
}