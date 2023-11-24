using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.services;

namespace NeoModLoader.General.Event;

public abstract class BaseListener
{
}
public abstract class AbstractListener<TListener, THandler> : BaseListener 
    where THandler : AbstractHandler<THandler> 
    where TListener : AbstractListener<TListener, THandler>
{
    protected static TListener instance { get; private set; }
    protected List<THandler> handlers { get; } = new();

    public AbstractListener()
    {
        instance = (TListener)this;
        Type type = GetType();
        Harmony.CreateAndPatchAll(type, type.FullName);
    }
    protected static void InsertCallHandleCode(List<CodeInstruction> codes, int pos)
    {
        codes.Insert(pos, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TListener), "HandleAll")));
    }
    public static void RegisterHandler(THandler handler)
    {
        instance.handlers.Add(handler);
    }
}