using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.api;

/// <summary>
///     This class is used to create a wide window
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AbstractWideWindow<T> : AbstractWindow<T> where T : AbstractWideWindow<T>
{
    /// <summary>
    /// Set size of the wide window
    /// </summary>
    /// <param name="pSize"></param>
    public void SetSize(Vector2 pSize)
    {
        Instance.BackgroundTransform.GetComponent<RectTransform>().sizeDelta = pSize;
        Instance.BackgroundTransform.parent.Find("CloseBackground").localPosition =
            new Vector3(pSize.x / 2 - 20, pSize.y / 2 + 7);
        Instance.BackgroundTransform.Find("TitleBackground").GetComponent<RectTransform>().sizeDelta =
            new Vector2(pSize.x / 2, 30);
        Instance.BackgroundTransform.Find("TitleBackground").localPosition = new Vector3(0, pSize.y / 2 + 5);
        Instance.GetComponent<ScrollWindow>().titleText.transform.localPosition = new Vector3(0, pSize.y / 2 + 5);
        Instance.GetComponent<ScrollWindow>().titleText.GetComponent<RectTransform>().sizeDelta =
            new Vector2(pSize.x / 2 * 0.92f, 28);
    }

    /// <summary>
    ///     以 pWindowId 创建并初始化一个 T 类型的窗口
    /// </summary>
    /// <param name="pWindowId"></param>
    /// <returns></returns>
    public static T CreateAndInit(string pWindowId, Vector2 pSize = default)
    {
        WindowId = pWindowId;
        if (pSize == default)
        {
            pSize = new Vector2(600, 280);
        }

        var scroll_window = WindowCreator.CreateEmptyWindow(pWindowId, pWindowId + " Title");

        var window_object = scroll_window.gameObject;
        Instance = window_object.AddComponent<T>();
        Instance.gameObject.SetActive(false);

        Instance.BackgroundTransform = scroll_window.transform.Find("Background");
        Instance.BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);

        Instance.ContentTransform = Instance.BackgroundTransform.Find("Scroll View/Viewport/Content");

        Instance.BackgroundTransform.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        Instance.BackgroundTransform.GetComponent<Image>().type = Image.Type.Sliced;
        //Instance.BackgroundTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 280);

        //Instance.BackgroundTransform.Find("CloseBackgound").localPosition = new Vector3(260, 147);

        var title_bg = new GameObject("TitleBackground", typeof(Image));
        title_bg.transform.SetParent(Instance.BackgroundTransform);
        title_bg.transform.localPosition = new Vector3(0, 145);
        title_bg.transform.localScale = Vector3.one;
        title_bg.transform.SetSiblingIndex(1);
        title_bg.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowBigCloseSliced();
        title_bg.GetComponent<Image>().type = Image.Type.Sliced;
        //title_bg.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 30);

        //scroll_window.titleText.transform.localPosition = new Vector3(0, 145);
        //scroll_window.titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 28);
        Instance.SetSize(pSize);
        Instance.Init();

        Instance.Initialized = true;

        return Instance;
    }
}