using System.Runtime.CompilerServices;
using HarmonyLib;

namespace NeoModLoader.General;

public static class LM
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static string Get(string key)
    {
        return LocalizedTextManager.getText(key);
    }
    public static void LoadLocale(string pLanguage, string pFilePath)
    {
        if (pFilePath.ToLower().EndsWith(".json"))
        {
            Dictionary<string, string> locale =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(pFilePath);

            if (locale == null)
            {
                throw new Exception($"Failed to load locale file at {pFilePath}");
            }

            foreach (var (key, value) in locale)
            {
                Add(pLanguage, key, value);
            }
        }
        /*
        else if (pFilePath.ToLower().EndsWith(".yml"))
        {
            
        }
        */
        else
        {
            throw new Exception("Unsupported file type");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static void AddToCurrentLocale(string key, string value)
    {
        if (string.IsNullOrEmpty(current_language))
        {
            current_language = LocalizedTextManager.instance.GetField<string>("language")!;
        }
        
        Add(current_language, key, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static void Add(string language, string key, string value)
    {
        if (!locales.ContainsKey(language))
        {
            locales[language] = new Dictionary<string, string>();
        }
        
        locales[language][key] = value;
    }

    private static Dictionary<string, string> localized_text;
    private static string current_language = "";
    private static Dictionary<string, Dictionary<string, string>> locales = new();
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ApplyLocale(string language)
    {
        if (!locales.ContainsKey(language))
        {
            return;
        }
        
        localized_text = LocalizedTextManager.instance.GetField<Dictionary<string, string>>("localizedText")!;
        
        foreach (var (key, value) in locales[language])
        {
            localized_text[key] = value;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizedTextManager), nameof(LocalizedTextManager.setLanguage))]
    internal static void setLanguagePostfix(string pLanguage)
    {
        ApplyLocale(pLanguage);
    }
}