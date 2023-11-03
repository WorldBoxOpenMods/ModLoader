//#define TEST

using System.Runtime.CompilerServices;
using NeoModLoader.constants;

namespace NeoModLoader.services;

public static class LogService
{
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
}