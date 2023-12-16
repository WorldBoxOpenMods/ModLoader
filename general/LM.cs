using System.Runtime.CompilerServices;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.services;
using Newtonsoft.Json;

namespace NeoModLoader.General;

/// <summary>
/// LM is short for Localization Manager
/// </summary>
public static class LM
{
    /// <summary>
    /// Store all locales loaded by NML.
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>> locales = new();

    /// <summary>
    /// Get localized text from key for current language
    /// </summary>
    /// <returns>Same with <see cref="LocalizedTextManager.getText"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static string Get(string key)
    {
        return LocalizedTextManager.getText(key);
    }

    /// <summary>
    /// Load locale from a stream (It must be a json file)
    /// </summary>
    /// <param name="pLanguage">Target save language</param>
    /// <param name="pStream">Stream of locale file</param>
    /// <exception cref="FormatException">Text in <see cref="pStream"/> is not in correct format to its file name extension</exception>
    public static void LoadLocale(string pLanguage, Stream pStream)
    {
        string text = new StreamReader(pStream).ReadToEnd();
        Dictionary<string, string> locale =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

        if (locale == null)
        {
            throw new FormatException($"Failed to load locale file for stream as json");
        }

        foreach (var (key, value) in locale.Select<KeyValuePair<string, string>, (string key, string value)>(pair =>
                     (pair.Key, pair.Value)))
        {
            Add(pLanguage, key, value);
        }
    }

    /// <summary>
    /// Load locale from a file
    /// </summary>
    /// <param name="pLanguage">Target save language</param>
    /// <param name="pFilePath">Path to locale file</param>
    /// <exception cref="UnsupportedFileTypeException">Only support <see cref="pFilePath"/> ends with ".json"</exception>
    /// <exception cref="FormatException">File at <see cref="pFilePath"/> is not in correct format to its file name extension</exception>
    public static void LoadLocale(string pLanguage, string pFilePath)
    {
        if (pFilePath.ToLower().EndsWith(".json"))
        {
            Dictionary<string, string> locale =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(pFilePath));

            if (locale == null)
            {
                throw new FormatException($"Failed to load locale file at {pFilePath} as json");
            }

            foreach (var (key, value) in locale.Select<KeyValuePair<string, string>, (string key, string value)>(pair =>
                         (pair.Key, pair.Value)))
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
            LogService.LogWarning($"Unsupported locale file type of path: {pFilePath}");
        }
    }

    /// <summary>
    /// Add a key-value pair to current locale
    /// <remarks>Overwrite if key exists</remarks>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static void AddToCurrentLocale(string key, string value)
    {
        LocalizedTextManager.instance.localizedText[key] = value;
        Add(LocalizedTextManager.instance.language, key, value);
    }

    /// <summary>
    /// Add a key-value pair to language locale
    /// <param name="language">Target language</param>
    /// <remarks>Overwrite if key exists</remarks>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
    public static void Add(string language, string key, string value)
    {
        if (!locales.ContainsKey(language))
        {
            locales[language] = new Dictionary<string, string>();
        }

        locales[language][key] = value;
    }

    /// <summary>
    /// Apply all locales loaded by this mod to target locale.
    /// <remarks>It will be called automatically by NML when language is changed.</remarks>
    /// </summary>
    /// <param name="language">Language to apply</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ApplyLocale(string language)
    {
        if (!locales.ContainsKey(language))
        {
            return;
        }

        foreach (var (key, value) in locales[language]
                     .Select<KeyValuePair<string, string>, (string key, string value)>(pair => (pair.Key, pair.Value)))
        {
            LocalizedTextManager.instance.localizedText[key] = value;
        }

        LocalizedTextManager.updateTexts();
    }

    /// <summary>
    /// Apply all locales loaded by this mod to current locale.
    /// <remarks>It will be called automatically by NML when language is changed.</remarks>
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ApplyLocale()
    {
        if (!locales.ContainsKey(LocalizedTextManager.instance.language))
        {
            return;
        }

        foreach (var (key, value) in locales[LocalizedTextManager.instance.language]
                     .Select<KeyValuePair<string, string>, (string key, string value)>(pair => (pair.Key, pair.Value)))
        {
            LocalizedTextManager.instance.localizedText[key] = value;
        }
    }

    /// <summary>
    /// Patch to <see cref="LocalizedTextManager.setLanguage"/>
    /// <remarks>Listen language change event</remarks>
    /// </summary>
    /// <param name="pLanguage">Parameter of <see cref="LocalizedTextManager.setLanguage"/></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizedTextManager), nameof(LocalizedTextManager.setLanguage))]
    internal static void setLanguagePostfix(string pLanguage)
    {
        ApplyLocale(pLanguage);
    }
}