namespace NeoModLoader.api;

/// <summary>
/// An interface for indicating that your mod handles feature loading via a dynamic <see cref="IModFeatureManager"/>.
/// </summary>
public interface IFeatureLoadManaged
{
    /// <summary>
    /// The <see cref="IModFeatureManager"/> that is managing the features of this mod.
    /// </summary>
    IModFeatureManager ModFeatureManager { get; }
}