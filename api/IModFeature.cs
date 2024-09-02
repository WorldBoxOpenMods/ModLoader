namespace NeoModLoader.api;

/// <summary>
/// An interface for mod features that should be loaded dynamically by a <see cref="IModFeatureManager"/>.
/// </summary>
public interface IModFeature
{
    /// <summary>
    /// A reference to the <see cref="IModFeatureManager"/> that is managing this <see cref="IModFeature"/>.
    /// </summary>
    IModFeatureManager ModFeatureManager { get; set; }
    /// <summary>
    /// A <see cref="ModFeatureRequirementList"/> of <see cref="Type"/>s of <see cref="IModFeature"/>s that are required for this <see cref="IModFeature"/> to function.
    /// </summary>
    ModFeatureRequirementList RequiredModFeatures { get; }
    /// <summary>
    /// A <see cref="ModFeatureRequirementList"/> of <see cref="Type"/>s of <see cref="IModFeature"/>s that this <see cref="IModFeature"/> might use if available but doesn't rely on. Can also be used to force a specific feature load order if possible.
    /// </summary>
    ModFeatureRequirementList OptionalModFeatures { get; }
    /// <summary>
    /// Initializes the <see cref="IModFeature"/>. This method should be called by the <see cref="IModFeatureManager"/> when the feature is loaded.
    /// This method can safely throw an exception without causing broader issues, as the <see cref="IModFeatureManager"/> is expected to catch and log such behaviour.
    /// </summary>
    /// <returns>Whether the feature has been loaded successfully.</returns>
    bool Init();

    /// <summary>
    /// Finalizes the initialization of the <see cref="IModFeature"/>. This method should be called after <see cref="Init"/> to perform any remaining setup tasks that might depend on other mods having loaded already.
    /// This method can safely throw an exception without causing broader issues, as the <see cref="IModFeatureManager"/> is expected to catch and log such behavior.
    /// </summary>
    /// <returns>Whether the feature has been successfully post-initialized.</returns>
    bool PostInit();
}