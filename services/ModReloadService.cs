using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using NeoModLoader.utils.Builders;

namespace NeoModLoader.services;

internal static class ModReloadService
{
    public static bool HotfixMethods(IReloadable pMod, ModDeclare pModDeclare)
    {
        if (!ModReloadUtils.Prepare(pMod, pModDeclare)) return false;
        if (!ModReloadUtils.CompileNew()) return false;
        if (!ModReloadUtils.PatchHotfixMethodsNT()) return false;
        return true;
    }

    public static bool ReloadResources(IMod pMod)
    {
        MasterBuilder Builder = new();
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.ModResourceFolderName), out List<Builder> builders);
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.NCMSAdditionModResourceFolderName), out List<Builder> builders2);
        Builder.AddBuilders(builders);
        Builder.AddBuilders(builders2);
        Builder.BuildAll();
        return false;
    }

    public static void ReloadLocales(IMod pMod)
    {
        ModCompileLoadService.LoadLocales(pMod, pMod.GetDeclaration(), true, true);
    }
}
