using System.Runtime.CompilerServices;

namespace IL2CPP2MONO;

public class App
{
    public static string InternalPath {  [MethodImpl(MethodImplOptions.InternalCall)] get; }
}
//we will add an IOS class used for detours once we make them good