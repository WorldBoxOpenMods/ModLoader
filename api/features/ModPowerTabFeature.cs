namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModObjectFeature{TObject}"/> that has the primary purpose to creating a specific object of type <see cref="PowersTab"/>.
/// </summary>
public abstract class ModPowerTabFeature : ModObjectFeature<PowersTab>
{
    /// <summary>
    /// A method for positioning a power button on the tab. This method should be overridden to position the button on the <see cref="PowersTab"/> that the feature produces.
    /// </summary>
    /// <param name="button">The <see cref="PowerButton"/> to position.</param>
    /// <returns>Whether positioning was a success.</returns>
    public abstract bool PositionButton(PowerButton button);
}