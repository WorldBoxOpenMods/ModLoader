using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.ui;

internal static class UIManager
{
    public static void init()
    {
        InformationWindow.CreateAndInit("Information");
        ModListWindow.CreateAndInit("NeoModList");
        WorkshopModListWindow.CreateAndInit("WorkshopMods");
        ModUploadWindow.CreateAndInit("ModUpload");
        ModUploadingProgressWindow.CreateAndInit("ModUploadingProgress");
        PowerButtonCreator.CreateWindowButton("NML_ModsList", "NeoModList", InternalResourcesGetter.GetIcon(),
            PowerTabs.Main, new Vector2(403.2f, -18));
    }
}