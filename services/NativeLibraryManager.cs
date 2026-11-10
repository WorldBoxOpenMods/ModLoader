using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using Newtonsoft.Json;
namespace NeoModLoader.services;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeLibraryManager
{
    // Windows
    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW",
        SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr WinLoadLibrary(string fileName);

    [DllImport("kernel32.dll", EntryPoint = "GetProcAddress",
        SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr WinGetAddress(IntPtr handle, string symbol);

    // Linux
    [DllImport("libdl.so.2", EntryPoint = "dlopen",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LinuxDlOpen(string filename, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlsym",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LinuxDlSym(IntPtr handle, string symbol);

    [DllImport("libdl.so.2", EntryPoint = "dlerror",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr LinuxDlError();

    // Android
    [DllImport("libmain.so", EntryPoint = "load_library",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr AndroidDlOpen(string filename);

    [DllImport("libdl.so", EntryPoint = "dlsym",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr AndroidDlSym(IntPtr handle, string symbol);

    private const int OPEN_FLAGS = 2 | 0x100;
    
    delegate IntPtr loadlib(string Path, out string Error);
    delegate IntPtr getaddress(IntPtr Handle, string symbol);

    static getaddress GetAddress;
    static loadlib Loadlib;
    struct NativeLibrary(IntPtr Handle, string Path)
    {
        public IntPtr Handle = Handle;
        public string Path = Path;
    }
    private static readonly ConcurrentDictionary<string, NativeLibrary>
        Libraries = new();
    
    private static string PlatformFolder;
    private static string SpecialPath;
    
    [Serializable]
    private class LoadOrder
    {
        public List<string> Order { get; set; }
    }

    static NativeLibraryManager()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                GetAddress = WinGetAddress;
                Loadlib = (path, out error) =>
                {
                    error = null;
                    IntPtr handle = WinLoadLibrary(path);
                    if (handle == IntPtr.Zero)
                    {
                        error = new System.ComponentModel.Win32Exception(
                            Marshal.GetLastWin32Error()).Message;
                    }

                    return handle;
                };
                PlatformFolder = "win";
                break;

            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
                GetAddress = LinuxDlSym;
                Loadlib = (path, out error) =>
                {
                    error = null;
                    IntPtr handle = LinuxDlOpen(path, OPEN_FLAGS);
                    if (handle == IntPtr.Zero)
                    {
                        error = Marshal.PtrToStringAnsi(LinuxDlError());
                    }
                    return handle;
                };
                PlatformFolder = "linux";
                break;

            case RuntimePlatform.Android:
                PlatformFolder = "android";
                SpecialPath = Path.Combine(IL2CPP2MONO.App.InternalPath, "NML");
                GetAddress = AndroidDlSym;
                Loadlib = (path, out error) =>
                {
                    error = ""; //it already logs the error inside libmain
                    IntPtr handle = AndroidDlOpen(path);
                    return handle;
                };
                break;
            
            default:
                PlatformFolder = null;
                break;
        }
    }

    public static void LoadLibraries(string Folder)
    {
        if (PlatformFolder == null)
        {
            throw new PlatformNotSupportedException("Native libraries are not supported on this platform!");
        }
        string nativeFolder = Path.Combine(Folder, PlatformFolder);
        if (!Directory.Exists(nativeFolder))
        {
            LogService.LogWarning(
                $"Warning: {Folder} doesn't contain any native libraries for the current platform!");
            return;
        }
        string[] files = Directory.GetFiles(nativeFolder);

        Dictionary<string, string> available =
            new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            available.Add(name, file);
        }

        List<string> loadOrder = GetOrder(
            Folder) ?? [..available.Keys];

        foreach (string name in loadOrder)
        {
            string file = available[name];

            if (Libraries.TryGetValue(name, out var lib))
            {
                if (string.Equals(lib.Path, file,
                        StringComparison.Ordinal))
                    continue;

                throw new InvalidOperationException(
                    $"Native library '{name}' has already been loaded from '{lib.Path}'. " +
                    $"Cannot load it again from '{file}'.");
            }

            string loadPath = file;
            if (SpecialPath != null)
            {
                Directory.CreateDirectory(SpecialPath);
                loadPath = Path.Combine(SpecialPath, Path.GetFileName(file));
                File.Copy(file, loadPath, true);
            }
            IntPtr handle = Loadlib(loadPath, out string error);
            if (handle == IntPtr.Zero)
            {
                throw new BadImageFormatException(
                    $"Failed to load native library '{file}': {error}");
            }
            Libraries[name] = new NativeLibrary(handle, file);
        }
    }

    public static T GetNativeFunction<T>(
        string Name, string Symbol) where T : Delegate
    {
        if (!Libraries.TryGetValue(Name, out var lib))
            throw new DllNotFoundException($"native library {Name} was not found!");
        IntPtr function = GetAddress(lib.Handle, Symbol);
        if (function == IntPtr.Zero)
        {
            throw new EntryPointNotFoundException(
                $"Could not find symbol '{Symbol}' in native library '{Name}'.");
        }
        return Marshal.GetDelegateForFunctionPointer(
            function, typeof(T)) as T;
    }

    // Dependency handling

    private static List<string> GetOrder(
        string root)
    {
        string manifestPath = Path.Combine(
            root, "order.json");
        if (!File.Exists(manifestPath))
            return null;
        try
        {
            string Json = File.ReadAllText(manifestPath);
            LoadOrder order = JsonConvert.DeserializeObject<LoadOrder>(Json);
            return order?.Order;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                "Failed to read order.json.", ex);
        }
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate IntPtr NMLAPIFunc(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string signature
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate bool NativeModEntry( [MarshalAs(UnmanagedType.FunctionPtr)] NMLAPIFunc NMLAPI, [MarshalAs(UnmanagedType.U8)] ModDeclare Declare);

    static ConcurrentDictionary<Delegate, IntPtr> Delegates = new();
    static IntPtr NMLAPI(string signature)
    {
        Assembly NML = WorldBoxMod.NeoModLoaderAssembly;
        string[] args = signature.Split(':');
        Type Type = NML.GetType(args[0]);
        Delegate Method = Type?.GetMethodDelegate(args[1], true);
        if (Method == null)
        {
            return IntPtr.Zero;
        }
        if (Delegates.TryGetValue(Method, out var lib))
        {
            return lib;
        }

        IntPtr ptr = Marshal.GetFunctionPointerForDelegate(Method);
        Delegates.TryAdd(Method, ptr);
        return ptr;
    }
    static readonly NMLAPIFunc NMLAPICallback = NMLAPI;
    /// <summary>
    /// native libraries which act as mods may use the NML API for assistance. the entry point is always bool ModEntry(IntPtr NMLAPIFUNC) 
    /// </summary>
    /// <remarks>NMLAPIFUNC is IntPtr (string signature). this returns a neomodloader static function which may be used</remarks>
    public static bool LoadNativeMod(string Name, ModDeclare Mod)
    {
        NativeModEntry EntryPoint = GetNativeFunction<NativeModEntry>(Name, "ModEntry");
        return EntryPoint(NMLAPICallback, Mod);
    }
}