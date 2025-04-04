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
    /// <param name="pWindowIcon">Path to window image after "ui/Icons"</param>
    /// <remarks>
    /// Prototype comes from [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    /// <returns></returns>
    public static ScrollWindow CreateEmptyWindow(string pWindowID, string pWindowTitleKey, string pWindowIcon = "neomodloader")
    {
        if (ScrollWindow._all_windows.TryGetValue(pWindowID, out ScrollWindow emptyWindow))
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

        ScrollWindow._all_windows[pWindowID] = window;
        window.create(true);
        
        var window_background = window.transform.Find("Background");
        window_background.Find("Scroll View").gameObject.SetActive(true);
        window_background.Find("Scroll View").GetComponent<RectTransform>().sizeDelta =
            new Vector2(232, 270);
        window_background.Find("Scroll View").localPosition = new Vector3(0, -6);
        window_background.Find("Scroll View/Viewport").GetComponent<RectTransform>().sizeDelta =
            new Vector2(30, 0);
        window_background.Find("Scroll View/Viewport").localPosition = new Vector3(-131, 135);

        AssetManager.window_library.add(new WindowAsset() {
            id = pWindowID,
            icon_path = pWindowIcon,
        });
        
        return window;
    }
}