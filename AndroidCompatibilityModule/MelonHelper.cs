namespace NeoModLoader.AndroidCompatibilityModule;
#if IL2CPP
using MelonLoader.Utils;
using MelonLoader;
#endif
public static class MelonHelper
{
    #if IL2CPP
    public static string GetPath()
    {
        return MelonEnvironment.GameRootDirectory;
    }

    public static void Log(string msg)
    {
        UnityEngine.Debug.Log(msg);
        MelonLogger.Msg(msg);
    }
    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogError(msg);
        MelonLogger.Error(msg);
    }
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
        MelonLogger.Warning(msg);
    }
    #else
       public static string GetPath()
    {
       return "";
    }
     public static void Log(string msg)
    {
      UnityEngine.Debug.Log(msg);
    }
    public static void LogError(string msg)
    {
       UnityEngine.Debug.LogError(msg);
    }
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }
    #endif
}