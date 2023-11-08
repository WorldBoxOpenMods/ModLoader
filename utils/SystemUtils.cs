using System.Runtime.InteropServices;

namespace NeoModLoader.utils;
/// <summary>
/// It contains methods which act outside Game and Loader
/// </summary>
public static class SystemUtils
{
    /// <summary>
    /// Run cmd.exe as admin, only works in Windows
    /// </summary>
    /// <param name="parameters">parameters passed into cmd</param>
    public static void CmdRunAs(string[] parameters)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = String.Join(" ", parameters);
        Console.WriteLine(startInfo.Arguments);
        startInfo.Verb = "runas";
        System.Diagnostics.Process.Start(startInfo);
    }
    /// <summary>
    /// Search all directories dirname filtered for files' fullpath with filename filtered 
    /// </summary>
    /// <param name="path">The root path to directory to search</param>
    /// <param name="fileNameJudge">File name filter</param>
    /// <param name="dirNameJudge">Directory name filter</param>
    /// <returns>All found files' fullpath(Path root reset to '/' or 'C:')</returns>
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