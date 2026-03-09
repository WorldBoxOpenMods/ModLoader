using System.Reflection;

namespace MonoMod.RuntimeDetour;
//do litterally nothing
public class Detour : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException("how did we get here?");
    }

    public Detour(MethodInfo info, MethodInfo neww)
    {
        throw new NotImplementedException("this is a STUB!");
    }
}