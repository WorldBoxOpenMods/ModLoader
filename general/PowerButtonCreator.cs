using JetBrains.Annotations;
using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.General;

public static class PowerButtonCreator
{
    /// <summary>
    /// Create a button used to open window
    /// </summary>
    /// <remarks>
    ///     Please set "{pId}"[Necessary] and "{pId} Description"[Optional] in locale file and load it.
    /// </remarks>
    /// <param name="pId">PowerButton's name, determines title and desc key of tooltip</param>
    /// <param name="pWindowId">Id of the window to open</param>
    /// <param name="pIcon"></param>
    /// <param name="pAttachTab"></param>
    /// <param name="pLocalPosition"></param>
    /// <returns></returns>
    public static PowerButton CreateWindowButton([NotNull]string pId, [NotNull]string pWindowId,
        Sprite pIcon, [CanBeNull]string pAttachTab = null, Vector2 pLocalPosition = default)
    {
        PowerButton prefab = ResourcesFinder.FindResources<PowerButton>("worldlaws")[0];
        
        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }
        PowersTab tab = GetTab(pAttachTab);
        PowerButton obj;
        if (tab == null)
        {
            obj = GameObject.Instantiate(prefab);
        }
        else
        {
            obj = GameObject.Instantiate(prefab, tab.transform);
            tab.powerButtons.Add(obj);
        }
        
        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }
        
        
        obj.name = pId;
        obj.icon.sprite = pIcon;
        obj.open_window_id = pWindowId;
        obj.type = PowerButtonType.Window;
        
        if(pLocalPosition == default && tab != null)
        {
            pLocalPosition = tab.GetNextButtonPosition();
        }

        var transform = obj.transform;
        
        transform.localPosition = pLocalPosition;
        transform.localScale = Vector3.one;
        
        obj.gameObject.SetActive(true);
        return obj;
    }
    public static PowersTab GetTab(string pId)
    {
        if (string.IsNullOrEmpty(pId)) return null;
        Transform tabTransform = CanvasMain.instance.canvas_ui.transform.Find(
            $"CanvasBottom/BottomElements/BottomElementsMover/CanvasScrollView/Scroll View/Viewport/Content/buttons/{pId}");
        
        if (tabTransform == null)
        {
            return null;
        }

        return tabTransform.GetComponent<PowersTab>();
    }
    /// <summary>
    /// Auto find empty position for button
    /// </summary>
    /// <remarks>
    ///     This method cost a lot of time, please use it carefully.
    /// </remarks>
    /// <param name="pTab">Search Tab</param>
    /// <returns></returns>
    public static Vector2 GetNextButtonPosition(this PowersTab pTab)
    {
        return default;
        foreach (PowerButton button in pTab.powerButtons)
        {
            
        }
    }

    public static void AddButtonToTab(PowerButton button, PowersTab tab, Vector2 position)
    {
        if (position == default)
        {
            position = tab.GetNextButtonPosition();
        }

        Transform transform;
        (transform = button.transform).SetParent(tab.transform);
        transform.localPosition = position;
        transform.localScale = Vector3.one;
        tab.powerButtons.Add(button);
    }
}