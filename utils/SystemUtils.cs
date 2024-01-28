using System.Diagnostics;
using NeoModLoader.services;

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
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = String.Join(" ", parameters);
        Console.WriteLine(startInfo.Arguments);
        startInfo.Verb = "runas";
        Process.Start(startInfo);
    }

    /// <summary>
    /// Run bash with parameters
    /// </summary>
    /// <param name="parameters"></param>
    public static void BashRun(string[] parameters)
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = "bash";
        startInfo.Arguments = String.Join(" ", parameters);
        Console.WriteLine(startInfo.Arguments);
        Process.Start(startInfo);
    }

    /// <summary>
    /// Search all directories dirname filtered for files' fullpath with filename filtered 
    /// </summary>
    /// <param name="path">The root path to directory to search</param>
    /// <param name="fileNameJudge">File name filter</param>
    /// <param name="dirNameJudge">Directory name filter</param>
    /// <returns>All found files' fullpath(Path root reset to '/' or 'C:')</returns>
    public static List<string> SearchFileRecursive(string             path, Func<string, bool> fileNameJudge,
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

            foreach (var subDir in dir.GetDirectories())
            {
                if (dirNameJudge(subDir.Name))
                {
                    queue.Enqueue(subDir);
                }
            }
        }

        return result;
    }

    public static void CopyDirectory(string pSource, string pTarget)
    {
        if (string.IsNullOrEmpty(pSource) || string.IsNullOrEmpty(pTarget))
        {
            LogService.LogWarning("Source or target is null or empty");
            LogService.LogStackTraceAsWarning();
            return;
        }

        if (!Directory.Exists(pSource))
        {
            LogService.LogWarning($"Source directory {pSource} does not exist");
            LogService.LogStackTraceAsWarning();
            return;
        }

        if (!Directory.Exists(pTarget)) Directory.CreateDirectory(pTarget);
        var queue = new Queue<string>();
        queue.Enqueue("");
        while (queue.Count > 0)
        {
            var relative_dir = queue.Dequeue();
            var source_dir = new DirectoryInfo(Path.Combine(pSource, relative_dir));
            var target_dir = new DirectoryInfo(Path.Combine(pTarget, relative_dir));
            if (!target_dir.Exists) target_dir.Create();
            foreach (FileInfo file in source_dir.GetFiles())
                file.CopyTo(Path.Combine(pTarget, relative_dir, file.Name), true);
            foreach (DirectoryInfo subDir in source_dir.GetDirectories())
                queue.Enqueue(Path.Combine(relative_dir, subDir.Name));
        }
    }
}