using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using NeoModLoader.services;

namespace NeoModLoader.utils;

/// <summary>
///     Subset parser for mod folder .gitignore files. Recognized syntax: comments, blank lines,
///     directory-only rules (trailing /), root anchors (leading /), the globs * and ? and the common
///     ** positions, plus negation rules (!) with git's last-match-wins ordering. Rules are
///     evaluated against paths relative to the mod folder.
/// </summary>
/// <remarks>
///     The rule set feeds source timestamp scanning, workshop upload packaging and mod code
///     collection, so whatever a mod author does not version control is not treated as source by
///     NML either. The built-in ignore lists in <see cref="constants.Paths" /> always apply on top.
/// </remarks>
internal static class GitIgnoreUtils
{
    private static readonly ConcurrentDictionary<string, (long IgnoreFileTime, GitIgnoreRuleSet RuleSet)> rule_set_cache = new();

    public static GitIgnoreRuleSet GetRuleSet(string folderPath)
    {
        var ignore_file_path = Path.Combine(folderPath, ".gitignore");
        long ignore_file_time = File.Exists(ignore_file_path)
            ? File.GetLastWriteTimeUtc(ignore_file_path).Ticks
            : 0;

        if (!rule_set_cache.TryGetValue(folderPath, out var cached)
            || cached.IgnoreFileTime != ignore_file_time)
        {
            cached = (ignore_file_time, Parse(folderPath, ignore_file_path));
            rule_set_cache[folderPath] = cached;
        }

        return cached.RuleSet;
    }

    private static GitIgnoreRuleSet Parse(string folderPath, string ignoreFilePath)
    {
        var rule_set = new GitIgnoreRuleSet();
        try
        {
            if (!File.Exists(ignoreFilePath)) return rule_set;

            foreach (var raw_line in File.ReadAllLines(ignoreFilePath))
            {
                var line = raw_line.TrimEnd();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                rule_set.AddRule(line);
            }
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to parse .gitignore of {folderPath}: {e.Message}");
        }

        return rule_set;
    }
}

internal sealed class GitIgnoreRuleSet
{
    private sealed class IgnoreRule
    {
        public Regex Regex;
        public bool  DirOnly;
        public bool  Negated;
    }

    private readonly List<IgnoreRule> _rules = new();

    public void AddRule(string line)
    {
        var negated = line.StartsWith("!");
        if (negated) line = line.Substring(1);
        var dir_only = line.EndsWith("/");
        var pattern = line.TrimEnd('/');
        var anchored = pattern.StartsWith("/");
        if (anchored) pattern = pattern.TrimStart('/');
        if (pattern.Length == 0) return;

        var regex = BuildRegex(pattern, anchored);
        if (regex == null) return;
        _rules.Add(new IgnoreRule { Regex = regex, DirOnly = dir_only, Negated = negated });
    }

    /// <summary>
    ///     Git semantics: rules apply in file order and the LAST matching rule decides. Note that
    ///     (as in git) a negation can only re-include a path whose parent directories are not
    ///     ignored, because an ignored directory is pruned before its children are evaluated.
    /// </summary>
    public bool IsIgnored(string relativePath, bool isDirectory)
    {
        if (Evaluate(relativePath, isDirectory)) return true;

        // A file below an ignored directory is ignored as well; the directory pruned walk usually
        // already prevents reaching here, this only keeps direct queries consistent.
        var slash_index = relativePath.LastIndexOf('/');
        while (slash_index > 0)
        {
            if (Evaluate(relativePath.Substring(0, slash_index), true)) return true;
            slash_index = relativePath.LastIndexOf('/', slash_index - 1);
        }

        return false;
    }

    private bool Evaluate(string relativePath, bool isDirectory)
    {
        var ignored = false;
        foreach (var rule in _rules)
        {
            if (rule.DirOnly && !isDirectory) continue;
            if (rule.Regex.IsMatch(relativePath)) ignored = !rule.Negated;
        }

        return ignored;
    }

    private static Regex BuildRegex(string pattern, bool anchored)
    {
        var builder = new StringBuilder();
        if (!anchored && !pattern.Contains('/'))
        {
            // A pattern without any slash matches a path segment at any depth.
            builder.Append("(?:^|.*/)");
            AppendGlobBody(builder, pattern);
            builder.Append('$');
        }
        else
        {
            builder.Append('^');
            AppendGlobBody(builder, pattern);
            builder.Append('$');
        }

        try
        {
            return new Regex(builder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static void AppendGlobBody(StringBuilder builder, string pattern)
    {
        var index = 0;
        while (index < pattern.Length)
        {
            var c = pattern[index];
            if (c == '*' && index + 1 < pattern.Length && pattern[index + 1] == '*')
            {
                if (index + 2 < pattern.Length && pattern[index + 2] == '/')
                {
                    builder.Append("(?:[^/]+/)*");
                    index += 3;
                    continue;
                }

                builder.Append(".*");
                index += 2;
                continue;
            }

            switch (c)
            {
                case '*':
                    builder.Append("[^/]*");
                    break;
                case '?':
                    builder.Append("[^/]");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '.':
                case '$':
                case '^':
                case '{':
                case '}':
                case '(':
                case ')':
                case '|':
                case '+':
                    builder.Append('\\').Append(c);
                    break;
                case '[':
                {
                    var close = pattern.IndexOf(']', index + 1);
                    if (close < 0)
                    {
                        builder.Append("\\[");
                        index++;
                    }
                    else
                    {
                        builder.Append(pattern.Substring(index, close - index + 1));
                        index = close;
                    }

                    break;
                }
                default:
                    builder.Append(c);
                    break;
            }

            index++;
        }
    }
}
