using System.Reflection;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.General.Event.Listeners;
using NeoModLoader.services;

namespace NeoModLoader.General.Event;

internal static class ListenerManager
{
    private static readonly string ListenerNamespace =
        $"{nameof(NeoModLoader)}.{nameof(General)}.{nameof(Event)}.{nameof(Listeners)}";

    private static readonly HashSet<BaseListener> _listeners = new();
    public static void _init()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            if(type.Namespace != ListenerNamespace) continue;

            try
            {
                var constructor =
                    type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                if (constructor == null)
                {
                    LogService.LogWarning($"Cannot find constructor of {type.FullName}");
                    continue;
                }
                if (constructor.Invoke(null) is not BaseListener listener)
                {
                    LogService.LogWarning($"Failed to construct listener instance of {type.FullName}");
                    continue;
                }
                _listeners.Add(listener);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to patch listener: {type.FullName}");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
        }
    }
}