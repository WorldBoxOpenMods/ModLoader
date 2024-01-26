using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.ui;

internal static class UIManager
{
    public static void init()
    {
        InformationWindow.CreateWindow("Information", "Information Title");
        ModListWindow.CreateAndInit("NeoModList");
        WorkshopModListWindow.CreateAndInit("WorkshopMods");
        ModUploadWindow.CreateAndInit("ModUpload");
        ModUploadingProgressWindow.CreateAndInit("ModUploadingProgress");
        ModUploadAuthenticationWindow.CreateAndInit("ModUploadAuthentication");
        ModConfigureWindow.CreateAndInit("ModConfigure");
        PowerButtonCreator.AddButtonToTab(
                                          PowerButtonCreator.CreateWindowButton("NML_ModsList", "NeoModList",
                                                                                    InternalResourcesGetter.GetIcon()),
                                          PowerButtonCreator.GetTab(PowerTabNames.Main),
                                          new Vector2(403.2f, -18));
    }
}