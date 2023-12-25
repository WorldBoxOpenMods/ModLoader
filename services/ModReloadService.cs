using JetBrains.Annotations;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;

namespace NeoModLoader.services;

internal static class ModReloadService
{
    public static bool HotfixMethods(IMod pMod)
    {
        if (!ModReloadUtils.Prepare(pMod)) return false;
        if (!ModReloadUtils.CompileNew()) return false;
        if (!ModReloadUtils.PatchHotfixMethods()) return false;
        return true;
    }

    public static bool ReloadResources(IMod pMod)
    {
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.ModResourceFolderName));
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.NCMSAdditionModResourceFolderName));
        
        return false;
    }

    public static void ReloadLocales(IMod pMod)
    {
        if (pMod is not ILocalizable localizable_mod)
            return;

        string locale_path = localizable_mod.GetLocaleFilesDirectory(pMod.GetDeclaration());
        if (!Directory.Exists(locale_path)) return;
        
        var files = Directory.GetFiles(locale_path);
        foreach (var locale_file in files)
        {
            LogService.LogInfo(
                $"Reload {locale_file} as {Path.GetFileNameWithoutExtension(locale_file)}");
            LM.LoadLocale(Path.GetFileNameWithoutExtension(locale_file), locale_file);
        }

        LM.ApplyLocale();
    }
}