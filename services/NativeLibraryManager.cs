using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.services;

public static class NativeLibraryManager
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "LoadLibrary")]
    private static extern IntPtr WinLoadLibrary(string lpFileName);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", SetLastError = true)]
    private static extern IntPtr WinGetAddress(IntPtr hModule, string procName);

    private const int RTLD_NOW = 2;
    private const int RTLD_GLOBAL = 8;
    [DllImport("libdl.so.2", EntryPoint = "dlopen", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr UnixDlOpen(string filename, int flags);
    [DllImport("libdl", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr UnixDlSym(IntPtr handle, string symbol);
    
    class UnixHandler(string FolderName, string SpecialPath) : PlatformNativeHandler
    {
        public void LoadNativeLibraries(string path)
        {
            
            string Folder = Path.Combine(path, FolderName);
            if (!Directory.Exists(Folder))
            {
                LogService.LogWarning($"Warning: {path} doesn't contain any native libraries for the current platform!");
                return;
            }
            foreach (var file in Directory.GetFiles(Folder))
            {
                IntPtr handle;
                if (SpecialPath != null) //android requires libraries in data/user
                {
                    string newfile = Path.Combine(SpecialPath, Path.GetFileName(file));
                    File.Copy(file, newfile, true);
                    handle = UnixDlOpen(newfile, RTLD_NOW | RTLD_GLOBAL);
                }
                else
                {
                    handle = UnixDlOpen(file, RTLD_NOW | RTLD_GLOBAL);
                }
                if (handle == IntPtr.Zero)
                {
                    throw new BadImageFormatException($"Failed to load native library {file}");
                }
                LibraryHandles.TryAdd(Path.GetFileNameWithoutExtension(file), handle);
            }
        }

        public IntPtr GetFunction(IntPtr Handle, string Name)
        {
          return UnixDlSym(Handle, Name);
        }
    }

    class NotSupportedHandler : PlatformNativeHandler
    {
        public void LoadNativeLibraries(string Folder)
        {
            throw new PlatformNotSupportedException("Native libraries are not supported on this platform!");
        }

        public IntPtr GetFunction(IntPtr Handle, string Name)
        {
            throw new PlatformNotSupportedException("Native libraries are not supported on this platform!");
        }
    }
    class WinHandler(string FolderName) : PlatformNativeHandler
    {
        public void LoadNativeLibraries(string path)
        {
            string Folder = Path.Combine(path, FolderName);
            if (!Directory.Exists(Folder))
            {
                LogService.LogWarning($"Warning: {path} doesn't contain any native libraries for the current platform!");
                return;
            }
            foreach (var file in Directory.GetFiles(Folder))
            {
                IntPtr handle = WinLoadLibrary(file);
                if (handle == IntPtr.Zero)
                {
                    throw new BadImageFormatException($"Failed to load native library {file}");
                }
                LibraryHandles.TryAdd(Path.GetFileNameWithoutExtension(file), handle);
            }
        }
        public IntPtr GetFunction(IntPtr Handle, string Name)
        {
           return WinGetAddress(Handle, Name);
        }
    }
    interface PlatformNativeHandler
    {
        void LoadNativeLibraries(string Folder);
        IntPtr GetFunction(IntPtr Handle, string Name);
    }

    private static ConcurrentDictionary<string, IntPtr> LibraryHandles = new ConcurrentDictionary<string, IntPtr>();
    private static PlatformNativeHandler Handler;

    static NativeLibraryManager()
    {
        Handler = Application.platform switch
        {
            RuntimePlatform.OSXEditor => new UnixHandler("osx", null),
            RuntimePlatform.OSXPlayer => new NotSupportedHandler(),
            RuntimePlatform.WindowsPlayer => new WinHandler("win"),
            RuntimePlatform.WindowsEditor => new WinHandler("win"),
            RuntimePlatform.IPhonePlayer => new NotSupportedHandler(),
            RuntimePlatform.Android => new UnixHandler("android", Path.Combine(Paths.SpecialFolderPath, "NML")),
            RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => new UnixHandler("linux", null),
            _ => new NotSupportedHandler()
        };
    }

    public static void LoadLibraries(string Folder)
    {
        Handler.LoadNativeLibraries(Folder);
    }

    public static T GetNativeFunction<T>(string Name, string Symbol) where T : Delegate
    {
        if (!LibraryHandles.TryGetValue(Name, out IntPtr Handle))
        {
            return null;
        }
        IntPtr Function = Handler.GetFunction(Handle, Name);
        return Marshal.GetDelegateForFunctionPointer(Function, typeof(T)) as T;
    }
    
}