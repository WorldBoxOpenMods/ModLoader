using NeoModLoader.utils.installers;

namespace NeoModLoader.services;

internal static class ExternalModInstallService
{
    public static async void CheckExternalModInstall()
    {
        var args = new List<string>(Environment.GetCommandLineArgs());
        args.RemoveAt(0);
        foreach (var arg in args) LogService.LogInfo(arg);

        var types = WorldBoxMod.NeoModLoaderAssembly.GetTypes();
        var cmd_installers =
            (from type in types
             where type.IsSubclassOf(typeof(ACmdModInstaller)) && !type.IsAbstract
             select (ACmdModInstaller)Activator.CreateInstance(type)).ToList();
        foreach (ACmdModInstaller installer in cmd_installers)
            for (var i = 0; i < args.Count; i++)
                if (await installer.CheckInstall(args[i]))
                    args.RemoveAt(i--);
    }
}