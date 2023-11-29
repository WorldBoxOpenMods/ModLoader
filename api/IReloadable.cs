namespace NeoModLoader.api;
/// <summary>
/// Implement this interface, your mod can reload assets manually.
/// </summary>
public interface IReloadable
{
    /// <summary>
    /// Reload assets in this method.
    /// </summary>
    public void Reload();
}