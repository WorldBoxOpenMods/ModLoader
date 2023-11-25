using UnityEngine;

namespace NeoModLoader.api;
/// <summary>
/// This interface is used to represent a mod.
/// <para>NeoModLoader searches all types of your code and find a class implements <see cref="IMod"/> and inherits <see cref="MonoBehaviour"/></para>
/// <para>Then creates component of the class and calls <see cref="OnLoad"/></para>
/// <para>Finally set the GameObject active</para>
/// </summary>
/// <remarks>
/// For a native mod, methods will be called in this order:
/// OnLoad -> Awake -> OnEnable -> Start -> Update
/// </remarks>
public interface IMod
{
    /// <summary>
    /// Get information of your mod.
    /// </summary>
    public ModDeclare GetDeclaration();
    /// <summary>
    /// Get the GameObject instance of your mod.
    /// </summary>
    public GameObject GetGameObject();
    /// <summary>
    /// Get the url to your mod's repository or community.
    /// </summary>
    public string GetUrl();
    /// <summary>
    /// This method will be called when the mod is loaded.
    /// </summary>
    /// <param name="pModDecl">Mod information</param>
    /// <param name="pGameObject">The GameObject instance of this mod</param>
    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject);
}