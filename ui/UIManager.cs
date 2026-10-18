using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.ui;

internal static class UIManager
{
    public static void init()
    {
        InformationWindow.CreateWindow("Information", "Information Title");
        ModUploadWindow.CreateAndInit("ModUpload");
        ModUploadingProgressWindow.CreateAndInit("ModUploadingProgress");
        ModUploadAuthenticationWindow.CreateAndInit("ModUploadAuthentication");
        ModConfigureWindow.CreateAndInit("ModConfigure");
        AIDisclosureWindow.CreateAndInit("AIDisclosureWindow");
        AIConfirmWindow.CreateAndInit("AIConfirmWindow");
        ModUploadModeWindow.CreateAndInit("ModUploadModeWindow");
        ModManagerWindow.CreateAndInit("ModManager");
        ModBrowserWindow.CreateAndInit("ModBrowser");
        ModInfoWindow.CreateAndInit("ModInfo");
        ConfirmWindow.CreateAndInit("NMLConfirm");
        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateWindowButton("NML_ModsList", "ModManager",
                                                  InternalResourcesGetter.GetIcon()),
            PowerButtonCreator.GetTab(PowerTabNames.Main),
          22);
    }
}