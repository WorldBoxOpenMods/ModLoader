namespace NeoModLoader.api;

/// <summary>
/// A standard implementation of <see cref="IModFeature"/> that provides barebone interface functionality in addition to extra methods for more convenient interop with the <see cref="IModFeatureManager"/>.
/// </summary>
public abstract class ModFeature : IModFeature
{
    /// <summary>
    /// A reference to the <see cref="IModFeatureManager"/> that is managing this <see cref="IModFeature"/>.
    /// </summary>
    public IModFeatureManager ModFeatureManager { get; set; }

    /// <summary>
    /// A virtual empty list to indicate a lack of required features. Override this property to add required features.
    /// </summary>
    public virtual List<Type> RequiredModFeatures { get; } = new List<Type>();
    /// <summary>
    /// A virtual empty list to indicate a lack of optional features. Override this property to add optional features or influence the feature load order.
    /// </summary>
    public virtual List<Type> OptionalModFeatures { get; } = new List<Type>();

    /// <inheritdoc cref="IModFeature.Init"/>
    public abstract bool Init();

    /// <summary>
    /// A utility method to check if a mod feature is loaded and get it if it is.
    /// </summary>
    /// <param name="feature">The variable that the feature gets stored into if it's loaded.</param>
    /// <typeparam name="T">The feature type that should be checked for. Any features you check for with this need to be either in your <see cref="RequiredModFeatures"/> or your <see cref="OptionalModFeatures"/>.</typeparam>
    /// <returns>Whether a feature of the provided type is loaded.</returns>
    protected bool TryGetFeature<T>(out T feature) where T : ModFeature
    {
        return ModFeatureManager.TryGetFeature(this, out feature);
    }

    /// <summary>
    /// A utility method to get a mod feature that is required for this feature to function.
    /// </summary>
    /// <typeparam name="T">The feature type that an instance should be provided of. Any features you request with this need to be in your <see cref="RequiredModFeatures"/>.</typeparam>
    /// <returns>A reference to an instance of the provided feature type.</returns>
    protected T GetFeature<T>() where T : ModFeature
    {
        return ModFeatureManager.GetFeature<T>(this);
    }

    /// <summary>
    /// A utility method to check if a mod feature is loaded.
    /// </summary>
    /// <typeparam name="T">The feature type to check for.</typeparam>
    /// <returns>Whether a feature of the provided type is loaded.</returns>
    protected bool IsFeatureLoaded<T>() where T : ModFeature
    {
        return ModFeatureManager.IsFeatureLoaded<T>();
    }
}