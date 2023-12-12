using NeoModLoader.General;

namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Localization
    {
        public static void Add(string key, string value)
        {
            LM.AddToCurrentLocale(key, value);
        }

        [Obsolete("Localization.addLocalization is deprecated, please use Localization.Add instead")]
        public static void addLocalization(string key, string value)
        {
            Add(key, value);
        }

        public static void Set(string key, string value)
        {
            Add(key, value);
        }

        [Obsolete("Localization.setLocalization is deprecated, please use Localization.Set instead")]
        public static void setLocalization(string key, string value)
        {
            Add(key, value);
        }

        public static void AddOrSet(string key, string value)
        {
            Add(key, value);
        }

        public static string Get(string key)
        {
            return LocalizedTextManager.instance.localizedText[key];
        }

        [Obsolete("Localization.getLocalization is deprecated, please use Localization.Get instead")]
        public static string getLocalization(string key)
        {
            return Get(key);
        }
    }
}