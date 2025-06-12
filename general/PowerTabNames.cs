namespace NeoModLoader.General;
/// <summary>
/// Object Names of tabs in vanilla game
/// </summary>
public static class PowerTabNames
{
    /// <summary>
    /// Object Name of the default tab in vanilla game
    /// </summary>
    public const string Main = "main";
    /// <summary>
    /// Object Name of the first tab in vanilla game
    /// </summary>
    public const string Drawing = "creation";
    /// <summary>
    /// Object Name of the second tab in vanilla game
    /// </summary>
    public const string Kingdoms = "noosphere";
    /// <summary>
    /// Object Name of the third tab in vanilla game
    /// </summary>
    public const string Creatures = "units";
    /// <summary>
    /// Object Name of the forth tab in vanilla game
    /// </summary>
    public const string Nature = "nature";
    /// <summary>
    /// Object Name of the fifth tab in vanilla game
    /// </summary>
    public const string Bombs = "destruction";
    /// <summary>
    /// Object Name of the sixth tab in vanilla game
    /// </summary>
    public const string Other = "other";
    /// <summary>
    /// Return a list of all tab names
    /// </summary>
    public static List<string> GetNames()
    {
        return new List<string>()
        {
            Main, Drawing, Kingdoms, Creatures, Nature, Bombs, Other
        };
    }
}