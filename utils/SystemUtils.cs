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

    public static void BashRun(string[] parameters)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "bash";
        startInfo.Arguments = String.Join(" ", parameters);
        Console.WriteLine(startInfo.Arguments);
        System.Diagnostics.Process.Start(startInfo);
    }

    public static List<string> SearchFileRecursive(string path, Func<string, bool> fileNameJudge,
        Func<string, bool> dirNameJudge)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        List<string> result = new List<string>();
        foreach (var file in dir.GetFiles())
        {
            if (fileNameJudge(file.Name))
            {
                result.Add(file.FullName);
            }
        }
        foreach(var subDir in dir.GetDirectories())
        {
            if (dirNameJudge(subDir.Name))
            {
                result.AddRange(SearchFileRecursive(subDir.FullName, fileNameJudge, dirNameJudge));
            }
        }

        return result;
    }
}