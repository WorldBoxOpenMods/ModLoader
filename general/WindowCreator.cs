using UnityEngine;

namespace NeoModLoader.General;

public static class WindowCreator
{
    private static Dictionary<string, ScrollWindow> _all_windows;
    private static List<LocalizedText> _all_localized_texts;

    internal static void init()
    {
        _all_windows = Reflection.GetStaticField<Dictionary<string, ScrollWindow>, ScrollWindow>("allWindows");
        _all_localized_texts = LocalizedTextManager.instance.GetField<List<LocalizedText>, LocalizedTextManager>("texts");
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

        LocalizedText titleText = window.titleText.GetComponent<LocalizedText>();
        titleText.key = pWindowTitleKey;
        _all_localized_texts.Add(titleText);
        
        ReflectionUtility.Reflection.CallMethod(window, "create", true);
        _all_windows[pWindowID] = window;

        return window;
    }
}