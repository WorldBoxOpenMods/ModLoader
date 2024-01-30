namespace NeoModLoader.api;

internal class ModCompilationCache
{
    public List<string> dependencies;
    public bool         disabled;
    public string       mod_id;
    public List<string> optional_dependencies;

    /// <summary>
    ///     The timestamp of the mod's last update
    /// </summary>
    public long timestamp;

    private ModCompilationCache()
    {
    }

    public ModCompilationCache(string pModID)
    {
        mod_id = pModID;
        timestamp = 0;
        dependencies = new List<string>();
        optional_dependencies = new List<string>();
    }

    public ModCompilationCache(ModDeclare pModDeclare, List<string> pDependencies, List<string> pOptionalDependencies)
    {
        mod_id = pModDeclare.UID;
        disabled = false;
        timestamp = 0;
        dependencies = new List<string>(pDependencies                  ?? new List<string>());
        optional_dependencies = new List<string>(pOptionalDependencies ?? new List<string>());
    }
}