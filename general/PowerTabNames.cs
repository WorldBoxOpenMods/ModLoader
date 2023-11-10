namespace NeoModLoader.General;
/// <summary>
/// Object Names of tabs in vanilla game
/// </summary>
public static class PowerTabNames
{
    /// <summary>
    /// Object Name of the default tab in vanilla game
    /// </summary>
    public const string Main = "Tab_Main";
    /// <summary>
    /// Object Name of the first tab in vanilla game
    /// </summary>
    public const string Drawing = "Tab_Drawing";
    /// <summary>
    /// Object Name of the second tab in vanilla game
    /// </summary>
    public const string Kingdoms = "Tab_Kingdoms";
    /// <summary>
    /// Object Name of the third tab in vanilla game
    /// </summary>
    public const string Creatures = "Tab_Creatures";
    /// <summary>
    /// Object Name of the forth tab in vanilla game
    /// </summary>
    public const string Nature = "Tab_Nature";
    /// <summary>
    /// Object Name of the fifth tab in vanilla game
    /// </summary>
    public const string Bombs = "Tab_Bombs";
    /// <summary>
    /// Object Name of the sixth tab in vanilla game
    /// </summary>
    public const string Other = "Tab_Other";

    public static List<string> GetNames()
    {
        return new List<string>()
        {
            Main, Drawing, Kingdoms, Creatures, Nature, Bombs, Other
        };
    }
}