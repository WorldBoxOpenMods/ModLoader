using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.services;

namespace NeoModLoader.General.Event;
/// <summary>
/// Basic Listener for unite all listener in a list
/// </summary>
public abstract class BaseListener
{
}
/// <summary>
/// Abstract listener for listening event
/// </summary>
/// <typeparam name="TListener"></typeparam>
/// <typeparam name="THandler">Event handler type</typeparam>
public abstract class AbstractListener<TListener, THandler> : BaseListener 
    where THandler : AbstractHandler<THandler> 
    where TListener : AbstractListener<TListener, THandler>
{
    /// <summary>
    /// Instance of this listener
    /// </summary>
    protected static TListener instance { get; private set; }
    /// <summary>
    /// All handlers
    /// </summary>
    protected List<THandler> handlers { get; } = new();
    /// <summary>
    /// Create a listener and register it to Harmony
    /// </summary>
    public AbstractListener()
    {
        instance = (TListener)this;
        Type type = GetType();
        Harmony.CreateAndPatchAll(type, type.FullName);
    }
    /// <summary>
    /// Simple insert code to call HandleAll method
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="pos">The position to insert</param>
    protected static void InsertCallHandleCode(List<CodeInstruction> codes, int pos)
    {
        codes.Insert(pos, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TListener), "HandleAll")));
    }
    /// <summary>
    /// Register handler. Do not register handler repeatedly.
    /// </summary>
    /// <param name="handler"></param>
    public static void RegisterHandler(THandler handler)
    {
        instance.handlers.Add(handler);
    }
}