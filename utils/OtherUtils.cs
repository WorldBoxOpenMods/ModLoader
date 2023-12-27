using System.Diagnostics;
using System.Text;

namespace NeoModLoader.utils;

/// <summary>
/// It provides some uncommon methods
/// </summary>
public static class OtherUtils
{
    /// <summary>
    /// Get StackTrace from this method, and skip the first <paramref name="skip_frames"/> frames
    /// </summary>
    /// <param name="skip_frames">The frame number to skip</param>
    /// <param name="indent">Each line start with several indent character</param>
    public static string GetStackTrace(int skip_frames = 0, string indent = "")
    {
        string stack_trace = Environment.StackTrace;
        StringBuilder sb = new StringBuilder();

        string[] lines = stack_trace.Split('\n');
        if (!string.IsNullOrEmpty(indent))
        {
            for (int i = skip_frames; i < lines.Length; i++)
            {
                sb.AppendLine(lines[i]);
            }
        }
        else
        {
            for (int i = skip_frames; i < lines.Length; i++)
            {
                for (int j = 0; j < i - skip_frames; j++)
                {
                    sb.Append(indent);
                }

                sb.AppendLine(lines[i]);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Search the stack trace to see if the method is called by <paramref name="pMethodName"/> in <paramref name="pTypeConstraint"/>
    /// </summary>
    /// <param name="pMethodName"></param>
    /// <param name="pTypeConstraint"></param>
    /// <param name="pSearchAll">Wheather search the entire stack trace</param>
    public static bool CalledBy(string pMethodName, Type pTypeConstraint, bool pSearchAll = false)
    {
        var stackTrace = new StackTrace();
        StackFrame[] frames = stackTrace.GetFrames();
        if (frames == null)
        {
            return false;
        }

        if (frames.Length < 3)
        {
            return false;
        }

        if (!pSearchAll)
        {
            return frames[2].GetMethod().Name == pMethodName && (frames[2].GetType() == pTypeConstraint ||
                                                                 frames[2].GetType().IsSubclassOf(pTypeConstraint));
        }

        for (int i = 2; i < frames.Length; i++)
        {
            if (frames[i].GetMethod().Name == pMethodName && (frames[i].GetType() == pTypeConstraint ||
                                                              frames[i].GetType().IsSubclassOf(pTypeConstraint)))
            {
                return true;
            }
        }

        return false;
    }
}