using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.services;

namespace NeoModLoader.General.Event;

public abstract class BaseListener
{
    public virtual void Patch()
    {
        Type type = GetType();
        Harmony.CreateAndPatchAll(type, type.FullName);
    }
}
public abstract class AbstractListener<T> : BaseListener where T : AbstractHandler<T>
{
    protected static void AppendCallHandleCode(List<CodeInstruction> codes, int pos)
    {
        codes.Insert(pos, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(T), nameof(AbstractHandler<T>.HandleAll))));
    }
}