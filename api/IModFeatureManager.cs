namespace NeoModLoader.api;

/// <summary>
/// An interface for a feature manager responsible for dynamically loading and managing <see cref="IModFeature"/>s.
/// </summary>
public interface IModFeatureManager : IStagedLoad
{
    /// <summary>
    /// A method to check if a feature of a specific type is loaded.
    /// </summary>
    /// <typeparam name="T">The type that should be checked for.</typeparam>
    /// <returns>Whether a feature of the specific type is loaded.</returns>
    bool IsFeatureLoaded<T>() where T : IModFeature;
    /// <summary>
    /// A method to get a feature of a specific type.
    /// </summary>
    /// <param name="askingModFeature">The feature that is asking for this information.</param>
    /// <typeparam name="T">The type that should be checked for. It is recommended that the <see cref="IModFeatureManager"/> ensures that the feature requesting a feature has said feature set as a requirement, and to throw an exception if this isn't the case.</typeparam>
    /// <returns>An instance of the requested mod feature.</returns>
    T GetFeature<T>(IModFeature askingModFeature) where T : IModFeature;
    /// <summary>
    /// A method to check if a feature of a specific type is loaded and get it if it is.
    /// </summary>
    /// <param name="askingModFeature">The feature that is asking for this information.</param>
    /// <param name="feature">The variable that the feature gets stored into if it's loaded.</param>
    /// <typeparam name="T">The type that should be checked for. It is recommended that the <see cref="IModFeatureManager"/> ensures that the feature requesting a feature has said feature set as a requirement or optional feature, and to throw an exception if this isn't the case.</typeparam>
    /// <returns>Whether a feature of the specific type is loaded.</returns>
    bool TryGetFeature<T>(IModFeature askingModFeature, out T feature) where T : IModFeature;
    /// <summary>
    /// A method that initializes the feature manager, and dynamically instantiates and initializes all <see cref="IModFeature"/>s in the correct required load order. It is assumed that this method properly accounts for any/all exceptions that individual features might throw during construction/initialization.
    /// </summary>
    void InstantiateFeatures();
}