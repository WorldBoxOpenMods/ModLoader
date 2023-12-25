namespace NeoModLoader.api;
/// <summary>
/// If your want your mod's locale files loaded automatically, implement this interface for your main class.
/// </summary>
public interface ILocalizable
{
    /// <param name="pModDeclare">Because your mod have not been loaded, you should use <see href="pModDeclare"/> instead of <see cref="IMod.GetDeclaration"/> to visit your mod's information</param>
    /// <returns>The path to the directory of your locale files</returns>
    public abstract string GetLocaleFilesDirectory(ModDeclare pModDeclare);
}