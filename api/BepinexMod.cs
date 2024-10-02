using UnityEngine;
namespace NeoModLoader.api;

/// <summary>
/// This class is used to represent BepInEx mods detected by NeoModLoader.
/// </summary>
public class BepinexMod : VirtualMod
{
    private MonoBehaviour _modComponent;

    /// Returns the MonoBehaviour that represents the mod.
    /// Might be null if NeoModLoader couldn't detect a mod instance.
    public MonoBehaviour GetModComponent()
    {
        return _modComponent;
    }
    
    /// <summary>
    /// A special version of OnLoad that also sets the mod component of a BepInEx mod.
    /// </summary>
    /// <param name="pModDecl">Mod information</param>
    /// <param name="pModComponent">The MonoBehaviour instance of this mod</param>
    public void OnLoad(ModDeclare pModDecl, MonoBehaviour pModComponent)
    {
        OnLoad(pModDecl, pModComponent?.gameObject);
        _modComponent = pModComponent;
    }
}