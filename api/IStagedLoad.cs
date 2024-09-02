namespace NeoModLoader.api;

/// <summary>
/// An interface for indicating that your mod is capable of loading in multiple stages to maximize compatibility with other mods.
/// <br></br>
/// <br></br>
/// Frame 1 after mod construction by NML: Pre-init phase, use for performing logic that's independent of the game or other mods in this stage. There's no declared method for this, as it's what OnModLoad() should be used for.
/// <br></br>
/// Frame 2: Init() is called, a method that teh mod caj use for everything that depends on the base game, but not on other mods.
/// <br></br>
/// Frame 3: PostInit() is called, a method that the mod can use for everything that depends on other mods having loaded already, like compatibility optimizations.
/// </summary>
public interface IStagedLoad
{
    /// <summary>
    /// Called two frames after mod construction, should be used for everything that depends on the base game, but not on other mods.
    /// </summary>
    void Init();
    /// <summary>
    /// Called three frames after mod construction, should be used for everything that depends on other mods having loaded already, like compatibility optimizations.
    /// </summary>
    void PostInit();
}