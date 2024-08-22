using System.Reflection.Emit;
using HarmonyLib;

namespace NeoModLoader.utils.instpredictors;

public class BaseInstPredictor
{
    private static readonly Dictionary<OpCode, HashSet<OpCode>> equal_opcodes = new();
    private readonly        Func<CodeInstruction, bool>         predicate;

    protected BaseInstPredictor()
    {
    }

    public BaseInstPredictor(OpCode pOpCode)
    {
        predicate = inst => OpcodeEquals(pOpCode, inst);
    }

    public BaseInstPredictor(object pOperand)
    {
        predicate = inst => inst.operand == pOperand;
    }

    public BaseInstPredictor(OpCode pOpCode, object pOperand)
    {
        predicate = inst => OpcodeEquals(pOpCode, inst) && inst.operand == pOperand;
    }

    public BaseInstPredictor(Func<CodeInstruction, bool> pPredicate)
    {
        predicate = pPredicate;
    }

    public virtual bool Predict(CodeInstruction pInst)
    {
        return predicate?.Invoke(pInst) ?? true;
    }

    protected static bool OpcodeEquals(OpCode pOpCode, OpCode pOpCodeAnother)
    {
        return pOpCodeAnother == pOpCode;
    }

    protected static bool OpcodeEquals(CodeInstruction pInst, CodeInstruction pInstAnother)
    {
        return pInst.opcode == pInstAnother.opcode ||
               (equal_opcodes.TryGetValue(pInst.opcode, out var set) && set.Contains(pInstAnother.opcode));
    }

    protected static bool OpcodeEquals(OpCode pOpCode, CodeInstruction pInst)
    {
        return pInst.opcode == pOpCode ||
               (equal_opcodes.TryGetValue(pOpCode, out var set) && set.Contains(pInst.opcode));
    }

    protected static bool OpcodeEquals(CodeInstruction pInst, OpCode pOpCode)
    {
        return pInst.opcode == pOpCode ||
               (equal_opcodes.TryGetValue(pOpCode, out var set) && set.Contains(pInst.opcode));
    }

    internal static void _init()
    {
        AddEqualOpCodes(OpCodes.Br,      OpCodes.Br_S);
        AddEqualOpCodes(OpCodes.Brtrue,  OpCodes.Brtrue_S);
        AddEqualOpCodes(OpCodes.Brfalse, OpCodes.Brfalse_S);
    }

    private static void AddEqualOpCodes(params OpCode[] pOpCodes)
    {
        foreach (OpCode code in pOpCodes)
        {
            if (!equal_opcodes.TryGetValue(code, out var set))
            {
                set = new HashSet<OpCode>();
                equal_opcodes[code] = set;
            }

            set.UnionWith(pOpCodes);

            foreach (OpCode second_iter in pOpCodes)
                if (equal_opcodes.TryGetValue(second_iter, out var second_set))
                    set.UnionWith(second_set);
        }
    }
}