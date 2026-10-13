using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.constants;
using NeoModLoader.utils;
using NeoModLoader.utils.Collections;

namespace NeoModLoader.services;
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DoNotDebug : Attribute{}
public static class DebugService
{
    static DebugService()
    {
        Logger = new Debugger<object[]>(AccessTools.Method(typeof(Hooks), nameof(Hooks.loghook)));
        ExceptionHandler = new Debugger<Exception>(null, null, null, AccessTools.Method(typeof(Hooks), nameof(Hooks.finalizer)));
        Profiler = new Debugger<long>(AccessTools.Method(typeof(Hooks), nameof(Hooks.prefix)), AccessTools.Method(typeof(Hooks), nameof(Hooks.postfix)));
    }
    class Hooks
    {
        public static void loghook(MethodBase __originalMethod, object[] __args)
        {
            Logger.Handler(__originalMethod, __args);
        }
        public static void prefix(out long __state)
        {
            __state = Stopwatch.GetTimestamp();
        }
        public static void postfix(
            MethodBase __originalMethod,
            long __state)
        {
            Profiler.Handler(__originalMethod, Stopwatch.GetTimestamp() - __state);
        }
        public static void finalizer(Exception __exception, MethodBase __originalMethod)
        {
            if (__exception is not null)
                ExceptionHandler.Handler(__originalMethod, __exception);
        }
    }
    public static bool IsDebuggable(MethodBase method)
    {
        if (method.IsAbstract || method.ContainsGenericParameters || method.IsSpecialName || method.Name.Contains("<"))
        {
            return false;
        }
        return true;
    }
    public class Debugger<T>
    {
        public void AddHandler(Action<MethodBase, T>  handler)
        {
            Handler += handler;
        }
        public Action<MethodBase, T> Handler;
        public Debugger(MethodInfo Prefix = null, MethodInfo Postfix = null, MethodInfo Transpiler = null, MethodInfo Finalizer = null)
        {
            if (Prefix is not null)
            {
                this.Prefix = new HarmonyMethod(Prefix);
            }
            if (Postfix is not null)
            {
                this.Postfix = new HarmonyMethod(Postfix);
            }
            if (Transpiler is not null)
            {
                this.Transpiler = new HarmonyMethod(Postfix);
            }
            if (Finalizer is not null)
            {
                this.Finalizer = new HarmonyMethod(Finalizer);
            }
        }
        protected HarmonyMethod Prefix;
        protected HarmonyMethod Postfix;
        protected HarmonyMethod Finalizer;
        protected HarmonyMethod Transpiler;
        public void Attach(Assembly assembly, Func<MethodBase, bool> predicate = null)
        {
            foreach (var Type in AccessTools.GetTypesFromAssembly(assembly))
            {
                Attach(Type, predicate);
            }
        }
        public void Attach(Type Type, Func<MethodBase, bool> predicate = null)
        {
            predicate ??= DefaultPredicate;
            foreach (var method in Type.GetMethods(
                         BindingFlags.Public |
                         BindingFlags.NonPublic |
                         BindingFlags.Instance |
                         BindingFlags.Static |
                         BindingFlags.DeclaredOnly).Where(m => IsDebuggable(m) && predicate(m)))
            {
                Attach(method);
            }
            foreach (var method in Type.GetConstructors(
                         BindingFlags.Public |
                         BindingFlags.NonPublic |
                         BindingFlags.Instance |
                         BindingFlags.Static |
                         BindingFlags.DeclaredOnly).Where(m => IsDebuggable(m) && predicate(m)))
            {
                Attach(method);
            }
        }
        public void Attach(MethodBase method)
        {
            try
            {
                Patcher.Patch(method, Prefix, Postfix, Transpiler, Finalizer, null);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to attach debugger to {method.FullDescription()} due to {e}");
            }
        }
    }
    static readonly Harmony Patcher = new (Others.harmony_id);
    public static readonly Debugger<object[]> Logger;
    public static readonly Debugger<Exception> ExceptionHandler;
    public static readonly Debugger<long> Profiler;
    public static readonly Func<MethodBase, bool> DefaultPredicate = method => !method.IsDefined(typeof(DoNotDebug), true) && !method.DeclaringType!.IsDefined(typeof(DoNotDebug), true);
}
public class HarmonyPatcher //any harmony patches causing you trouble? this lets you single them out!
{
    public Action<Type> Logger;
    public Action<Exception> Handler; //always invoked after the logger
    private HashList<Type> types = new();
    private Harmony harmony;
    public int Remaining => types.Count;
    
    public HarmonyPatcher(string ID)
    {
        harmony = new Harmony(ID);
    }
    public HarmonyPatcher(string ID, Assembly assembly)
    {
        harmony = new Harmony(ID);
        Add(assembly);
    }
    public void Add(Assembly assembly)
    {
        AccessTools.GetTypesFromAssembly(assembly).Do(type => Add(type));
    }
    public bool Add(Type type)
    {
        if (!type.HasPatches())
        {
            return false;
        }
        types.Add(type);
        return true;
    }
    /// <summary>
    /// patches a type only if it is in the list
    /// </summary>
    public void Patch(Type type)
    {
        if (types.Remove(type))
        {
            patch(type);
        }
    }
    /// <summary>
    /// patches a number of types
    /// </summary>
    /// <returns>true if there are still types remaining</returns>
    public bool Patch(int Num)
    {
        for (int i = 0; i < Num; i++)
        {
            if (!PatchNext(out Type _))
            {
                return false;
            }
        }
        return true;
    }
    void patch(Type type)
    {
        Logger?.Invoke(type);
        try
        {
            harmony.CreateClassProcessor(type, true).Patch();
        }
        catch (Exception e)
        {
           Handler?.Invoke(e);
        }
    }
    public void PatchAll()
    {
        foreach (var type in types)
        {
           patch(type);
        }
        types.Clear();
    }
    public void Sort(IComparer<Type> comparer)
    {
        types.Sort(comparer);
    }
    public IEnumerable<Type> Types => types;
    /// <summary>
    /// patches using the next type
    /// </summary>
    public bool PatchNext(out Type type)
    {
        type = null;
        if (types.Count == 0)
        {
            return false;
        }
        type = types[types.Count - 1];
        Patch(type);
        return true;
    }
    public bool PatchRandom(out Type type)
    {
        type = null;
        if (types.Count == 0)
        {
            return false;
        }
        type = types.GetRandom();
        Patch(type);
        return true;
    }
}