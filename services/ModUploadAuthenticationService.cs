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
            // TODO: Authenticate here
            while (true)
            {

                if (ModUploadAuthenticationWindow.Instance.AuthSkipped || !ModUploadAuthenticationWindow.Instance.Opened())
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
                        promise.Resolve();
                        break;
                    }
                }
            }

        }).Start();
        
        return promise;
    }
}