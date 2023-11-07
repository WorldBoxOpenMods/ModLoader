using System.Diagnostics;
using System.Text;

namespace NeoModLoader.utils;
/// <summary>
/// It provides some uncommon methods
/// </summary>
public static class OtherUtils
{
    /// <summary>
    /// Get StackTrace from this method, and skip the first <paramref name="skipFrames"/> frames
    /// </summary>
    /// <param name="skipFrames">The frame number to skip</param>
    /// <param name="indent">Each line start with several indent character</param>
    public static string GetStackTrace(int skipFrames = 0, char indent = '\t')
    {
        StringBuilder sb = new StringBuilder();
        var stackTrace = new System.Diagnostics.StackTrace();
        StackFrame[] frames = stackTrace.GetFrames();
        if (frames == null)
        {
            return "";
        }
        for(int i = skipFrames; i < frames.Length; i++)
        {
            sb.Append(new string(indent, i - skipFrames));
            sb.AppendLine(frames[i].ToString());
        }
        return sb.ToString();
    }
}