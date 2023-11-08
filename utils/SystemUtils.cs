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

    public static List<string> SearchFileRecursive(string path, Func<string, bool> fileNameJudge,
        Func<string, bool> dirNameJudge)
    {
        List<string> result = new List<string>();
        Queue<DirectoryInfo> queue = new Queue<DirectoryInfo>();
        queue.Enqueue(new DirectoryInfo(path));
        while (queue.Count > 0)
        {
            DirectoryInfo dir = queue.Dequeue();
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
                    queue.Enqueue(subDir);
                }
            }
        }

        return result;
    }
}