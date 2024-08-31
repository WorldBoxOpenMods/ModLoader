namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModObjectFeature{TObject}"/> that has the primary purpose to creating a specific object of type <see cref="PowerButton"/>.
/// </summary>
/// <typeparam name="TPowersTabFeature">A feature with the purpose of producing the <see cref="PowersTab"/> which the <see cref="PowerButton"/> should automatically be added to.</typeparam>
public abstract class ModButtonFeature<TPowersTabFeature> : ModObjectFeature<PowerButton> where TPowersTabFeature : ModPowerTabFeature
{
    /// <inheritdoc/>
    public override ModFeatureRequirementList RequiredModFeatures => base.RequiredModFeatures + typeof(TPowersTabFeature);
    /// <summary>
    /// A shorthand for grabbing the <see cref="PowersTab"/> that the <see cref="PowerButton"/> should be added to.
    /// </summary>
    protected PowersTab Tab => GetFeature<TPowersTabFeature>();
    /// <summary>
    /// An implementation of <see cref="ModObjectFeature{TObject}.Init"/> that initializes the button and adds it to the tab.
    /// </summary>
    /// <returns>Whether the <see cref="PowerButton"/> was successfully created and positioned on its assigned <see cref="PowersTab"/>.</returns>
    public override bool Init()
    {
        return base.Init() && GetFeature<TPowersTabFeature>().PositionButton(Object);
    }
}