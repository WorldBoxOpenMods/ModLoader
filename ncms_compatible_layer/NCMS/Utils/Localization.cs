using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoModLoader.General;

namespace NCMS.Utils
{
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
            return LM.Get(key);
        }

        [Obsolete("Localization.getLocalization is deprecated, please use Localization.Get instead")]
        public static string getLocalization(string key)
        {
            return Get(key);
        }
    }
}
