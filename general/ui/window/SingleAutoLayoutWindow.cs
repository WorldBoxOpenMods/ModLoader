using NeoModLoader.services;

namespace NeoModLoader.General.UI.Window;

public abstract class SingleAutoLayoutWindow<T> : AutoLayoutWindow<T> where T : AutoLayoutWindow<T>
{
    public static T      Instance { get; private set; }
    public static string WindowId => Instance.WindowID;

    public new static T CreateWindow(string pWindowID, string pWindowTitleKey)
    {
        if (Instance != null)
        {
            LogService.LogError("Cannot create more than one instance of this window.");
            return Instance;
        }

        Instance = AutoLayoutWindow<T>.CreateWindow(pWindowID, pWindowTitleKey);

        return Instance;
    }
}