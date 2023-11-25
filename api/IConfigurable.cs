namespace NeoModLoader.api;
/// <summary>
/// A mod that implements this interface will be configurable in the game and the config user interface will be automatically generated.
/// </summary>
public interface IConfigurable
{
    /// <summary>
    /// Get the config instance of your mod.
    /// <remarks>
    /// <list type="bullet">
    /// <item>The config instance is used to read and write config in user interface.</item>
    /// <item>You should manage its save and load manually if you implements <see cref="IConfigurable"/> yourself instead of from <see cref="BasicMod{T}"/></item>
    /// </list>
    /// </remarks>
    /// </summary>
    public abstract ModConfig GetConfig();
}