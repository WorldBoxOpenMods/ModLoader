namespace NeoModLoader.api;
/// <summary>
/// If your mod main class implements this, the method <see cref="OnUnload"/> will be called when the mod is disabled.
/// </summary>
public interface IUnloadable
{
    /// <summary>
    /// The method will be called when the mod is disabled.
    /// </summary>
    public void OnUnload();
}