using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoModLoader.General;
using UnityEngine;

namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Windows
    {
        private static Dictionary<string, ScrollWindow> _all_windows;

        internal static void init()
        {
            WindowCreator.init();
            _all_windows = Reflection.GetStaticField<Dictionary<string, ScrollWindow>, ScrollWindow>("allWindows");
        }
        public static ScrollWindow GetWindow(string pWindowID)
        {
            return WindowCreator.GetWindow(pWindowID);
        }

        public static ScrollWindow CreateNewWindow(string pWindowID, string pWindowTitleKey)
        {
            return WindowCreator.CreateEmptyWindow(pWindowID, pWindowTitleKey);
        }
    }
}
