using UnityEngine;

namespace NeoModLoader.General;

public static class WindowCreator
{
    private static Dictionary<string, ScrollWindow> _all_windows;

    internal static void init()
    {
        _all_windows = Reflection.GetStaticField<Dictionary<string, ScrollWindow>, ScrollWindow>("allWindows");
    }
    public static ScrollWindow GetWindow(string pWindowID)
    {
        return ScrollWindow.get(pWindowID);
    }

    public static ScrollWindow CreateEmptyWindow(string pWindowID, string pWindowTitleKey)
    {
        if (_all_windows.TryGetValue(pWindowID, out ScrollWindow emptyWindow))
        {
            return emptyWindow;
        }

        ScrollWindow window = UnityEngine.Object.Instantiate(Resources.Load<ScrollWindow>("windows/empty"), CanvasMain.instance.transformWindows);
        window.screen_id = pWindowID;
        window.name = pWindowID;

        window.titleText.GetComponent<LocalizedText>().key = pWindowTitleKey;

        ReflectionUtility.Reflection.CallMethod(window, "create", true);
        _all_windows[pWindowID] = window;

        return window;
    }
}