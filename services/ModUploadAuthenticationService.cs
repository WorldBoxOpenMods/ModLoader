using NeoModLoader.ui;
using NeoModLoader.utils.authentication;
using RSG;

namespace NeoModLoader.services;

/// <summary>
///     Authentication service for mod upload.
/// </summary>
public static class ModUploadAuthenticationService
{
    /// <summary>
    ///     Whether the user has authenticated.
    /// </summary>
    public static bool Authed { get; private set; } = false;

    /// <summary>
    ///     Start auto authenticating task
    /// </summary>
    public static void AutoAuth()
    {
        new Task(() =>
        {
            int i = 0;
            foreach (var auto_auth_func in ModUploadAuthenticationWindow.all_auto_auth_funcs)
            {
                try
                {
                    LogService.LogInfoConcurrent($"Trying auto auth at {i}...");
                    Authed = auto_auth_func();
                    if (Authed)
                    {
                        LogService.LogInfoConcurrent("Auto auth success!");
                        return;
                    }
                    else
                    {
                        LogService.LogInfoConcurrent($"Failed auto auth at {i}.");
                    }
                }
                catch (Exception e)
                {
                    // Display? It's only auto auth, so I think it's not necessary.
                    LogService.LogInfoConcurrent($"Failed auto auth at {i}: {e.Message}");
                }
                finally
                {
                    i++;
                }
            }
        }).Start();
    }

    /// <summary>
    ///     Start authentication window and watch for the result.
    /// </summary>
    /// <returns>The Promise, Resolve: successfully authenticate/skip, Reject: Cancel authentication</returns>
    public static Promise Authenticate()
    {
        Promise promise = new Promise();
        if (Authed)
        {
            new Task(() =>
            {
                Thread.Sleep(100);
                promise.Resolve();
            }).Start();
            return promise;
        }

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
                    bool auth_result;
                    try
                    {
                        auth_result = ModUploadAuthenticationWindow.Instance.AuthFunc();
                    }
                    catch (AuthenticaticationException e)
                    {
                        // TODO: Handle the error in some way
                        ModUploadAuthenticationWindow.SetState(false, e.Message);
                        continue;
                    }

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