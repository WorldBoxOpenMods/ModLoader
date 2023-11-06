using System.Runtime.InteropServices;

namespace NeoModLoader.utils;

public static class SystemUtils
{
    public static void CmdRunAs(string[] parameters)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = String.Join(" ", parameters);
        Console.WriteLine(startInfo.Arguments);
        startInfo.Verb = "runas";
        System.Diagnostics.Process.Start(startInfo);
    }
}