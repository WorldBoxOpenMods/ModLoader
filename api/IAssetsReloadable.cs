namespace NeoModLoader.api;
/// <summary>
/// Implement this interface, your mod can reload assets manually.
/// </summary>
public interface IAssetsReloadable
{
    /// <summary>
    /// Reload assets in this method.
    /// </summary>
    public void ReloadAssets();
}