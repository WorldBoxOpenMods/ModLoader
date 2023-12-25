using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;
/// <summary>
/// A listener at the end of <see cref="ClanManager.newClan"/> method.
/// </summary>
public class ClanCreateListener : AbstractListener<ClanCreateListener, ClanCreateHandler>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pClan"></param>
    /// <param name="pActor"></param>
    protected static void HandleAll(Clan pClan, Actor pActor)
    {
        StringBuilder sb = null;
        int idx = 0;
        int count = instance.handlers.Count;
        bool finished = false;
        while (!finished)
        {
            try
            {
                for (; idx < count; idx++)
                {
                    instance.handlers[idx].Handle(pClan, pActor);
                }
                finished = true;
            }
            catch (Exception e)
            {
                instance.handlers[idx].HitException();
                sb ??= new();
                sb.AppendLine($"Failed to handle event in {instance.handlers[idx].GetType().FullName}");
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                idx++;
            }
        }
        if(sb != null)
        {
            LogService.LogError(sb.ToString());
        }
    }
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ClanManager), nameof(ClanManager.newClan))]
    private static IEnumerable<CodeInstruction> _newClan_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = 6;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Dup));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        
        InsertCallHandleCode(codes, insert_index);
        return codes;
    }

    [Obsolete("Operation is not supported", true)]
    private static MethodInfo _createHandleAllMethodByIL()
    {
        MethodInfo handle_once = AccessTools.Method(typeof(ClanCreateHandler), nameof(ClanCreateHandler.Handle));

        var parameters = handle_once.GetParameters();
        List<Type> parameters_types = new();
        foreach (var parameter in parameters)
        {
            parameters_types.Add(parameter.ParameterType);
        }
        
        DynamicMethod dm = new("ClanCreateListener_HandleAll", typeof(void), parameters_types.ToArray());
        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Nop);
        // sb = null
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stloc_0);   
        // idx = 0
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc_1);   
        // count = instance.handlers.Count
        il.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.instance)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.handlers)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<ClanCreateHandler>), nameof(List<ClanCreateHandler>.Count)));
        il.Emit(OpCodes.Stloc_2);
        // finished = false
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc_3);   
        
        Label while_loop_entry = il.DefineLabel();
        Label while_loop_body_label = il.DefineLabel();
        Label for_loop_entry = il.DefineLabel();
        Label for_loop_body_label = il.DefineLabel();
        Label out_try = il.DefineLabel();
        
        il.Emit(OpCodes.Br, while_loop_entry); // while (!finished)
        il.MarkLabel(while_loop_body_label);
        il.Emit(OpCodes.Nop);
        // try{
        il.Emit(OpCodes.Nop);

        il.Emit(OpCodes.Br_S, for_loop_entry);
        il.MarkLabel(for_loop_body_label);
        il.Emit(OpCodes.Nop);
        // instance.handlers[idx]
        il.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.instance)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.handlers)));
        il.Emit(OpCodes.Ldloc_1); // idx
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(List<ClanCreateHandler>), "get_Item"));
        // .Handle(parameters)
        for (int i = 0; i < parameters.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i);
        }
        il.Emit(OpCodes.Callvirt, handle_once);

        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Nop);
        // idx++
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc_1);
        // idx <= count
        il.MarkLabel(for_loop_entry);
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldloc_2);
        il.Emit(OpCodes.Clt);
        il.Emit(OpCodes.Stloc_S, (byte)4);
        il.Emit(OpCodes.Ldloc_S, (byte)4);
        il.Emit(OpCodes.Brtrue_S, for_loop_body_label); // end of for loop
        // finished = true
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stloc_3);
        
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Leave_S, out_try);
        //}catch(Exception e){
        il.Emit(OpCodes.Stloc_S, (byte)5);
        il.Emit(OpCodes.Nop);
        
        // instance.handlers[idx]
        il.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.instance)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.handlers)));
        il.Emit(OpCodes.Ldloc_1); // idx
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(List<ClanCreateHandler>), "get_Item"));
        // .HitException()
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(ClanCreateHandler), nameof(ClanCreateHandler.HitException)));
        il.Emit(OpCodes.Nop);

        Label sb_not_null = il.DefineLabel();
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Brtrue_S, sb_not_null); // if(sb != null) goto sb_not_null;
        // sb = new StringBuilder()
        il.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
        il.Emit(OpCodes.Stloc_0);
        
        il.MarkLabel(sb_not_null);
        // sb.AppendLine("Failed to handle event in" + instance.handlers[idx].GetType().FullName)
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldstr, "Failed to handle event in");
        il.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.instance)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ClanCreateListener), nameof(ClanCreateListener.handlers)));
        il.Emit(OpCodes.Ldloc_1); // idx
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(List<ClanCreateHandler>), "get_Item"));
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(object), nameof(object.GetType)));
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Type), nameof(Type.FullName)));
        il.Emit(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), new Type[] { typeof(string), typeof(string) }));
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), new Type[] { typeof(string) }));
        il.Emit(OpCodes.Pop);
        // sb.AppendLine(e.Message)
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldloc_S, (byte)5);
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Exception), nameof(Exception.Message)));
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), new Type[] { typeof(string) }));
        il.Emit(OpCodes.Pop);
        // sb.AppendLine(e.StackTrace)
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldloc_S, (byte)5);
        il.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Exception), nameof(Exception.StackTrace)));
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), new Type[] { typeof(string) }));
        il.Emit(OpCodes.Pop);
        
        // idx++
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc_1);
        // } // end of catch
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Leave_S, out_try);
        
        il.MarkLabel(out_try);
        il.Emit(OpCodes.Nop);
        
        il.MarkLabel(while_loop_entry);
        il.Emit(OpCodes.Ldloc_3);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Stloc_S, (byte)6);
        il.Emit(OpCodes.Ldloc_S, (byte)6);
        il.Emit(OpCodes.Brtrue_S, while_loop_body_label); // end of while loop
        
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Cgt_Un);
        il.Emit(OpCodes.Stloc_S,(byte)7);
        il.Emit(OpCodes.Ldloc_S,(byte)7);
        
        Label sb_null = il.DefineLabel();
        il.Emit(OpCodes.Brfalse_S, sb_null); // if(sb == null) goto sb_null;
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.ToString)));
        il.Emit(OpCodes.Call, AccessTools.Method(typeof(LogService), nameof(LogService.LogError), new Type[] { typeof(string) }));
        il.Emit(OpCodes.Nop);
        
        il.MarkLabel(sb_null);
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ret);
        
        Delegate method = dm.CreateDelegate(typeof(Delegate));
        return method.GetMethodInfo();
    }
}