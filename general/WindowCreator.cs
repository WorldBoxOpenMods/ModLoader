using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.General;

/// <summary>
///     This class is used to create different windows
/// </summary>
public static class WindowCreator
{
    internal static void init()
    {
    }

    /// <summary>
    /// Create an empty window with a title auto localized
    /// </summary>
    /// <param name="pWindowID">It should be unique, suggest start with your own mod's UUID</param>
    /// <param name="pWindowTitleKey">It should be unique, suggest start with your own mod's UUID</param>
    /// <remarks>
    /// Prototype comes from [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    /// <returns></returns>
    public static ScrollWindow CreateEmptyWindow(string pWindowID, string pWindowTitleKey)
    {
        if (ScrollWindow.allWindows.TryGetValue(pWindowID, out ScrollWindow emptyWindow))
        {
            return emptyWindow;
        }

        ScrollWindow window = Object.Instantiate(Resources.Load<ScrollWindow>("windows/empty"),
            CanvasMain.instance.transformWindows);
        window.screen_id = pWindowID;
        window.name = pWindowID;

        LocalizedText titleText = window.titleText.GetComponent<LocalizedText>();
        titleText.key = pWindowTitleKey;
        LocalizedTextManager.instance.texts.Add(titleText);

        ScrollWindow.allWindows[pWindowID] = window;
        window.create(true);

        return window;
    }
}