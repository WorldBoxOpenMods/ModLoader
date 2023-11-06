using RSG;

namespace NeoModLoader.services;

public static class ModUploadAuthenticationService
{
    public static Promise Authenticate()
    {
        Promise promise = new Promise();

        new Task(() =>
        {
            // TODO: Authenticate here
            promise.Reject(new Exception("message"));   // If failed.
            promise.Resolve();                          // If success
        });
        
        return promise;
    }
}