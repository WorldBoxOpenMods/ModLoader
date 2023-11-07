using NeoModLoader.ui;
using RSG;

namespace NeoModLoader.services;

public static class ModUploadAuthenticationService
{
    public static bool Authed { get; private set; } = false;

    public static Promise Authenticate()
    {
        Promise promise = new Promise();

        ScrollWindow.showWindow(ModUploadAuthenticationWindow.WindowId);
        new Task(() =>
        {
            while (true)
            {
                if (!ModUploadAuthenticationWindow.Instance.Opened())
                {
                    promise.Reject(new Exception("Canceled"));
                    break;
                }
                if (ModUploadAuthenticationWindow.Instance.AuthSkipped)
                {
                    promise.Resolve();
                    break;
                }
                
                if (ModUploadAuthenticationWindow.Instance.AuthFuncSelected)
                {
                    // TODO: Maybe memory error when Auth Button clicked too fast?
                    ModUploadAuthenticationWindow.Instance.AuthFuncSelected = false;
                    bool auth_result = ModUploadAuthenticationWindow.Instance.AuthFunc();
                    if (auth_result)
                    {
                        Authed = true;
                        ModUploadAuthenticationWindow.SetState(true);
                        promise.Resolve();
                        break;
                    }
                    ModUploadAuthenticationWindow.SetState(false);
                }
            }

        }).Start();
        
        return promise;
    }
}