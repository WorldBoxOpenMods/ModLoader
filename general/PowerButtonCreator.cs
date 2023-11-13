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
    /// <param name="pIcon">The icon of the button</param>
    /// <param name="pParent">Which transform the button attached to</param>
    /// <param name="pLocalPosition">The button position in <see cref="pParent"/></param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateWindowButton([NotNull]string pId, [NotNull]string pWindowId,
        Sprite pIcon, [CanBeNull]Transform pParent = null, Vector2 pLocalPosition = default)
    {
        PowerButton prefab = ResourcesFinder.FindResource<PowerButton>("worldlaws");
        
        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }
        PowerButton obj;
        if (pParent == null)
        {
            obj = GameObject.Instantiate(prefab);
        }
        else
        {
            obj = GameObject.Instantiate(prefab, pParent);
        }
        
        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }
        
        
        obj.name = pId;
        obj.icon.sprite = pIcon;
        obj.open_window_id = pWindowId;
        obj.type = PowerButtonType.Window;

        var transform = obj.transform;
        
        transform.localPosition = pLocalPosition;
        transform.localScale = Vector3.one;
        
        obj.gameObject.SetActive(true);
        return obj;
    }
    /// <summary>
    /// Get a tab by its Object Name
    /// </summary>
    /// <param name="pId">The Name of the tab to find</param>
    /// <returns>Tab found, null if not find</returns>
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
    /// Add a button to a tab
    /// </summary>
    public static void AddButtonToTab(PowerButton button, PowersTab tab, Vector2 position)
    {
        Transform transform;
        (transform = button.transform).SetParent(tab.transform);
        transform.localPosition = position;
        transform.localScale = Vector3.one;
        tab.powerButtons.Add(button);
    }
}