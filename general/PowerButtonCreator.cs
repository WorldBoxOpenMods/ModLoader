using JetBrains.Annotations;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    /// <para>Prototype comes from NCMS</para>
    /// </remarks>
    /// <param name="pId">PowerButton's name, determines title and desc key of tooltip</param>
    /// <param name="pWindowId">Id of the window to open</param>
    /// <param name="pIcon">The icon of the button</param>
    /// <param name="pParent">Which transform the button attached to</param>
    /// <param name="pLocalPosition">The button position in <paramref name="pParent"/></param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateWindowButton([NotNull] string pId, [NotNull] string pWindowId,
        Sprite pIcon, [CanBeNull] Transform pParent = null, Vector2 pLocalPosition = default)
    {
        PowerButton prefab = ResourcesFinder.FindResource<PowerButton>("world_laws");

        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }

        PowerButton obj;
        if (pParent == null)
        {
            obj = Object.Instantiate(prefab);
        }
        else
        {
            obj = Object.Instantiate(prefab, pParent);
        }

        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }


        obj.name = pId;
        obj.icon.sprite = pIcon;
        obj.icon.overrideSprite = pIcon;
        obj.open_window_id = pWindowId;
        obj.type = PowerButtonType.Window;

        var transform = obj.transform;

        transform.localPosition = pLocalPosition;
        transform.localScale = Vector3.one;

        obj.gameObject.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Create a simple power button with click action
    /// </summary>
    /// <remarks>
    ///     Please set "{pId}"[Necessary] and "{pId} Description"[Optional] in locale file and load it.
    /// <para>Prototype comes from NCMS</para>
    /// </remarks>
    /// <param name="pId">PowerButton's name, determines title and desc key of tooltip</param>
    /// <param name="pAction">The action of the button</param>
    /// <param name="pIcon">The icon of the button</param>
    /// <param name="pParent">Which transform the button attached to</param>
    /// <param name="pLocalPosition">The button position in <paramref name="pParent"/></param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateSimpleButton([NotNull] string pId, UnityAction pAction,
        Sprite pIcon, [CanBeNull] Transform pParent = null, Vector2 pLocalPosition = default)
    {
        var prefab = ResourcesFinder.FindResource<PowerButton>("world_laws");

        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }

        PowerButton obj;
        obj = pParent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, pParent);

        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }


        obj.name = pId;
        obj.icon.sprite = pIcon;
        obj.icon.overrideSprite = pIcon;
        obj.type = PowerButtonType.Library;
        if (pAction != null) obj.GetComponent<Button>().onClick.AddListener(pAction);

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
    /// <param name="pLocalPosition">The button position in <paramref name="pParent"/></param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateGodPowerButton(string pGodPowerId, Sprite pIcon,
        [CanBeNull] Transform pParent = null, Vector2 pLocalPosition = default)
    {
        PowerButton prefab = ResourcesFinder.FindResource<PowerButton>("inspect");

        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }

        PowerButton obj;
        obj = pParent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, pParent);

        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }


        obj.name = pGodPowerId;
        obj.icon.sprite = pIcon;
        obj.icon.overrideSprite = pIcon;
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
    /// <param name="pLocalPosition">The button position in <paramref name="pParent"/></param>
    /// <param name="pNoAutoSetToggleAction">Not set god power's toggle_action automatically if it's not null</param>
    /// <returns>The PowerButton created</returns>
    public static PowerButton CreateToggleButton(string pGodPowerId, Sprite pIcon, [CanBeNull] Transform pParent = null,
        Vector2 pLocalPosition = default, bool pNoAutoSetToggleAction = false)
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
                PlayerConfig.instance.data.add(_option);
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
        else if (!pNoAutoSetToggleAction)
        {
            god_power.toggle_action = (PowerToggleAction)Delegate.Combine(god_power.toggle_action,
                new PowerToggleAction(toggleOption));
        }

        if (!PlayerConfig.dict.TryGetValue(god_power.toggle_name, out var option))
        {
            AssetManager.options_library.add(new OptionAsset()
            {
                id = god_power.toggle_name,
                default_bool = false,
                type = OptionType.Bool
            });
            option = PlayerConfig.instance.data.add(new PlayerOptionData(god_power.toggle_name)
            {
                boolVal = false
            });
        }

        var prefab = ResourcesFinder.FindResource<PowerButton>("kings_and_leaders");

        bool found_active = prefab.gameObject.activeSelf;
        if (found_active)
        {
            prefab.gameObject.SetActive(false);
        }

        PowerButton obj;
        obj = pParent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, pParent);

        if (found_active)
        {
            prefab.gameObject.SetActive(true);
        }


        obj.name = pGodPowerId;
        obj.icon.sprite = pIcon;
        obj.icon.overrideSprite = pIcon;
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

        return tabTransform == null ? null : tabTransform.GetComponent<PowersTab>();
    }

    /// <summary>
    /// Add a button to a tab
    /// </summary>
    [Obsolete("Specifying a position vector has become useless in 0.50.5, tab order is now determined by sibling index.")]
    public static void AddButtonToTab(PowerButton button, PowersTab tab, Vector2 position, int? siblingIndex = null)
    {
        // WorldBox has seemingly switched to a new tab system that overwrites localPosition and orders by sibling order
        AddButtonToTab(button, tab, siblingIndex);
    }

    /// <summary>
    /// Add a button to a tab
    /// </summary>
    public static void AddButtonToTab(PowerButton button, PowersTab tab, int? siblingIndex = null)
    {
        Transform transform;
        (transform = button.transform).SetParent(tab.transform);
        transform.localScale = Vector3.one;
        if (siblingIndex.HasValue) transform.SetSiblingIndex(siblingIndex.Value);
        tab._power_buttons.Add(button);
    }
}