using NeoModLoader.General;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Localization
    {
        public static void Add(string key, string value)
        {
            LM.AddToCurrentLocale(key, value);
            //LM.ApplyLocale(false); It is applied actually above.
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
            // There is a mod uses exception to check if the key exists and do something. So keep it.
            return LocalizedTextManager.instance._localized_text[key];
        }

        [Obsolete("Localization.getLocalization is deprecated, please use Localization.Get instead")]
        public static string getLocalization(string key)
        {
            return Get(key);
        }
    }
}