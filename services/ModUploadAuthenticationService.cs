using RSG;

namespace NeoModLoader.services;

public static class ModUploadAuthenticationService
{
    public static bool Authed { get; private set; } = true;

    public static void AutoAuth()
    {
        Authed = true;
    }

    public static Promise Authenticate()
    {
        Promise promise = new Promise();
        Authed = true;
        promise.Resolve();
        return promise;
    }
}