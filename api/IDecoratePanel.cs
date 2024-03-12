using NeoModLoader.ui.prefabs;

namespace NeoModLoader.api;

/// <summary>
///     Panel of a mod's information that can be decorated through its self's implementation.
/// </summary>
public interface IDecoratePanel
{
    /// <summary>
    ///     Implement this method to decorate the panel by your self.
    /// </summary>
    /// <param name="pPanel"></param>
    public void DecoratePanel(ModInfoPanel pPanel);
}