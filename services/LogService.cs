//#define TEST

using System.Runtime.CompilerServices;
using NeoModLoader.constants;
using NeoModLoader.utils;

namespace NeoModLoader.services;
/// <summary>
/// It is a service to log message to console or Unity Console
/// </summary>
public static class LogService
{
    /// <summary>
    /// Log Error message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.LogError("[NML]: " + message);
        }
        else
        {
            System.Console.Error.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log Warning message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.LogWarning("[NML]: " + message);
        }
        else
        {
            System.Console.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInfo(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.Log("[NML]: " + message);
        }
        else
        {
            System.Console.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Info
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsInfo()
    {
        LogInfo(OtherUtils.GetStackTrace(2));
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Warning
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsWarning()
    {
        LogWarning(OtherUtils.GetStackTrace(2));
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Error
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsError()
    {
        LogError(OtherUtils.GetStackTrace(2));
    }
}