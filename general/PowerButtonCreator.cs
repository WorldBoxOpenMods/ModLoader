using JetBrains.Annotations;
using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.General;
/// <summary>
/// This class is used to create power buttons easily
/// </summary>
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
    /// Create a button to use common god power
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Please set "{pGodPowerId}"[Necessary] and "{pGodPowerId} Description"[Optional] in locale file and load it. </item>
    /// <item>You should create and add the god power to <see cref="AssetManager.powers"/> before call this method</item>
    /// </list>
    /// </remarks>
    /// <param name="pGodPowerId">The god power's id bind to the button</param>
    /// <param name="pIcon">The icon of the button</param>
    /// <param name="pParent">Which transform the button attached to</param>
    /// <param name="pLocalPosition">The button position in &lt;see cref="pParent"/&gt;</param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateGodPowerButton(string pGodPowerId, Sprite pIcon, [CanBeNull]Transform pParent = null, Vector2 pLocalPosition = default)
    {
        PowerButton prefab = ResourcesFinder.FindResource<PowerButton>("inspect");
        
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
        
        
        obj.name = pGodPowerId;
        obj.icon.sprite = pIcon;
        obj.open_window_id = null;
        obj.type = PowerButtonType.Active;
        // More settings for it

        var transform = obj.transform;
        
        transform.localPosition = pLocalPosition;
        transform.localScale = Vector3.one;
        
        obj.gameObject.SetActive(true);
        return obj;
    }
    /// <summary>
    /// Create a button to use toggle god power
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Please set "{pGodPowerId}"[Necessary] and "{pGodPowerId} Description"[Optional] in locale file and load it. </item>
    /// <item>You should create and add the god power to <see cref="AssetManager.powers"/> before call this method</item>
    /// <item>You should not set <see cref="GodPower.toggle_action"/> = <see cref="PowerLibrary.toggleOption"/> of the god power</item>
    /// </list>
    /// </remarks>
    /// <param name="pGodPowerId">The god power's id bind to the button</param>
    /// <param name="pIcon">The icon of the button</param>
    /// <param name="pParent">Which transform the button attached to</param>
    /// <param name="pLocalPosition">The button position in &lt;see cref="pParent"/&gt;</param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateToggleButton(string pGodPowerId, Sprite pIcon, [CanBeNull]Transform pParent = null, Vector2 pLocalPosition = default)
    {
        GodPower god_power = AssetManager.powers.get(pGodPowerId);
        if (god_power == null)
        {
            LogService.LogError("Cannot find GodPower with id " + pGodPowerId);
            return null;
        }
        
        void toggleOption(string pPower)
        {
            GodPower power = AssetManager.powers.get(pPower);
            WorldTip.instance.showToolbarText(power);

            if (!PlayerConfig.dict.TryGetValue(power.toggle_name, out var _option))
            {
                _option = new PlayerOptionData(power.toggle_name)
                {
                    boolVal = false
                };
                PlayerConfig.dict.Add(power.toggle_name, _option);
            }
            
            _option.boolVal = !_option.boolVal;
            if (_option.boolVal && power.map_modes_switch)
                AssetManager.powers.disableAllOtherMapModes(pPower);
            PlayerConfig.saveData();
        }
        if (god_power.toggle_action == null)
        {
            god_power.toggle_action = toggleOption;
        }
        else
        {
            god_power.toggle_action = (PowerToggleAction)Delegate.Combine(god_power.toggle_action,
                new PowerToggleAction(toggleOption));
        }
        if(!PlayerConfig.dict.TryGetValue(god_power.toggle_name, out var option))
        {
            option = new PlayerOptionData(god_power.toggle_name)
            {
                boolVal = false
            };
            PlayerConfig.dict.Add(god_power.toggle_name, option);
        }
        
        PowerButton prefab = ResourcesFinder.FindResource<PowerButton>("kingsAndLeaders");
        
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
        
        
        obj.name = pGodPowerId;
        obj.icon.sprite = pIcon;
        obj.open_window_id = null;
        obj.type = PowerButtonType.Special;
        obj.transform.Find("ToggleIcon").GetComponent<ToggleIcon>().updateIcon(option.boolVal);
        LogService.LogInfo($"Set {obj.name} toggle to {option.boolVal}");
        // More settings for it

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