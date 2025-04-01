using NeoModLoader.General;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class Windows
    {
        public static Dictionary<string, ScrollWindow> AllWindows;

        internal static void init()
        {
            AllWindows = ScrollWindow._all_windows;
        }

        public static ScrollWindow GetWindow(string pWindowID)
        {
            return ScrollWindow._all_windows.TryGetValue(pWindowID, out var window) ? window : null;
        }

        public static ScrollWindow CreateNewWindow(string pWindowID, string pWindowTitle)
        {
            if (!LocalizedTextManager.stringExists(pWindowID))
            {
                LM.AddToCurrentLocale(pWindowID, pWindowTitle);
            }

            ScrollWindow window = WindowCreator.CreateEmptyWindow(pWindowID, pWindowID);
            window.gameObject.transform.Find("Background/Title").GetComponent<LocalizedText>()
                .setKeyAndUpdate(pWindowID);
            window.gameObject.transform.Find("Background/Title").GetComponent<LocalizedText>().autoField = false;
            return window;
        }

        public static void ShowWindow(string pWindowID)
        {
            ScrollWindow.showWindow(pWindowID);
        }
    }
}