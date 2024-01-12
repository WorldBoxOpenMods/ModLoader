using System.Runtime.CompilerServices;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.constants;
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

    private static readonly Dictionary<string, string> str2esc = new()
    {
        {
            "\\n", "\n"
        },
        {
            "\\r", "\r"
        },
        {
            "\\t", "\t"
        },
        {
            "\\b", "\b"
        },
        {
            "\\f", "\f"
        },
        {
            "\\\"", "\""
        },
        {
            "\\\'", "\'"
        },
        {
            "\\\\", "\\"
        },
        {
            "\\0", "\0"
        }
    };

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
    ///     Load locales from a file(Only support csv file)
    /// </summary>
    /// <param name="pFilePath">Path to csv file</param>
    /// <exception cref="FormatException">Invalid csv file</exception>
    public static void LoadLocales(string pFilePath)
    {
        if (pFilePath.ToLower().EndsWith(".csv"))
        {
            Dictionary<string, Dictionary<string, string>> locale = ParseCSV(File.ReadAllText(pFilePath));

            if (locale == null)
            {
                throw new FormatException($"Failed to load locale file at {pFilePath} as csv");
            }

            foreach (var language in locale.Keys)
            {
                var dict = locale[language];
                foreach (var k in dict.Keys)
                {
                    Add(language, k, dict[k]);
                }
            }
        }
        else
        {
            LogService.LogWarning($"Unsupported locale file type of path: {pFilePath}");
        }
    }

    /// <summary>
    ///     Load locales from a text stream(Only support csv text)
    /// </summary>
    /// <param name="pStream">Stream of a csv text</param>
    /// <exception cref="FormatException">Invalid csv text</exception>
    public static void LoadLocales(Stream pStream)
    {
        string text = new StreamReader(pStream).ReadToEnd();
        Dictionary<string, Dictionary<string, string>> locale = ParseCSV(text);

        if (locale == null)
        {
            throw new FormatException("Failed to load locale file for stream as csv");
        }

        foreach (var language in locale.Keys)
        {
            var dict = locale[language];
            foreach (var k in dict.Keys)
            {
                Add(language, k, dict[k]);
            }
        }
    }

    private static Dictionary<string, Dictionary<string, string>> ParseCSV(string pText)
    {
        var lines = pText.Split('\n');

        if (lines.Length < 2) return null;
        if (string.IsNullOrEmpty(lines[0].Trim())) return null;
        if (!lines[0].Contains(',')) return null;

        var languages = lines[0].Split(',');
        var locale = new Dictionary<string, Dictionary<string, string>>();
        for (int i = 1; i < languages.Length; i++)
        {
            locale[languages[i]] = new Dictionary<string, string>();
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;
            if (!lines[i].Contains(',')) continue;
            var line = str2esc.Keys.Aggregate(lines[i], (current, key) => current.Replace(key, str2esc[key]))
                .Split(',');
            var key = line[0];

            if (string.IsNullOrEmpty(key)) continue;

            for (int j = 1; j < line.Length; j++)
            {
                locale[languages[j]][key] = line[j];
            }
        }

        return locale;
    }

    /// <summary>
    /// Load locale from a stream (It must be a json file)
    /// </summary>
    /// <param name="pLanguage">Target save language</param>
    /// <param name="pStream">Stream of locale file</param>
    /// <exception cref="FormatException">Text in <see name="pStream"/> is not in correct format to its file name extension</exception>
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
    /// <exception cref="UnsupportedFileTypeException">Only support <see name="pFilePath"/> ends with ".json"</exception>
    /// <exception cref="FormatException">File at <see name="pFilePath"/> is not in correct format to its file name extension</exception>
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
    /// <param name="key"></param>
    /// <param name="value"></param>
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
    /// <param name="pUpdateTexts"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ApplyLocale(string language, bool pUpdateTexts = true)
    {
        if (!locales.ContainsKey(language))
        {
            locales[language] = new Dictionary<string, string>();
        }

        foreach (var (key, value) in locales[language]
                     .Select<KeyValuePair<string, string>, (string key, string value)>(pair => (pair.Key, pair.Value)))
        {
            LocalizedTextManager.instance.localizedText[key] = value;
        }

        foreach (var key in locales[CoreConstants.DefaultLocaleID].Keys
                     .Where(key => !LocalizedTextManager.instance.localizedText.ContainsKey(key)))
            LocalizedTextManager.instance.localizedText[key] = locales[CoreConstants.DefaultLocaleID][key];

        LocalizedTextManager.updateTexts();
    }

    /// <summary>
    /// Apply all locales loaded by this mod to current locale.
    /// <remarks>It will be called automatically by NML when language is changed.</remarks>
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ApplyLocale(bool pUpdateTexts = true)
    {
        if (!locales.ContainsKey(LocalizedTextManager.instance.language))
        {
            locales[LocalizedTextManager.instance.language] = new Dictionary<string, string>();
        }

        foreach (var (key, value) in locales[LocalizedTextManager.instance.language]
                     .Select<KeyValuePair<string, string>, (string key, string value)>(pair => (pair.Key, pair.Value)))
        {
            LocalizedTextManager.instance.localizedText[key] = value;
        }

        foreach (var key in locales[CoreConstants.DefaultLocaleID].Keys
                     .Where(key => !LocalizedTextManager.instance.localizedText.ContainsKey(key)))
            LocalizedTextManager.instance.localizedText[key] = locales[CoreConstants.DefaultLocaleID][key];

        if (pUpdateTexts) LocalizedTextManager.updateTexts();
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