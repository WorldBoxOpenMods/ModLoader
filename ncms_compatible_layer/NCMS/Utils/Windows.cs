using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoModLoader.General;
using UnityEngine;

namespace NCMS.Utils
{
    public class Windows
    {
        private static Dictionary<string, ScrollWindow> _all_windows;

        internal static void init()
        {
            _all_windows = Reflection.GetStaticField<Dictionary<string, ScrollWindow>, ScrollWindow>("allWindows");
        }
        public static ScrollWindow GetWindow(string pWindowID)
        {
            return _all_windows.TryGetValue(pWindowID, out ScrollWindow value) ? value : null;
        }

        public static ScrollWindow CreateNewWindow(string pWindowID, string pWindowTitleKey)
        {
            if (_all_windows.ContainsKey(pWindowID))
            {
                return _all_windows[pWindowID];
            }

            ScrollWindow window = UnityEngine.Object.Instantiate(Resources.Load<ScrollWindow>("windows/empty"), CanvasMain.instance.transformWindows);
            window.screen_id = pWindowID;
            window.name = pWindowID;

            window.titleText.GetComponent<LocalizedText>().key = pWindowTitleKey;
            _all_windows[pWindowID] = window;

            return window;
        }
    }
}
