using HarmonyLib;
using NeoModLoader.utils.instpredictors;

namespace NeoModLoader.utils;

/// <summary>
///     Utility class for Harmony Transpiler
/// </summary>
public static class HarmonyUtils
{
    /// <summary>
    ///     Find a code snippet in a list of instructions
    /// </summary>
    /// <param name="pCodes"></param>
    /// <param name="pResult">The code snippet found</param>
    /// <param name="pSnippetPredictors"></param>
    /// <returns>Index of the start of the code snippet in <paramref name="pCodes" /></returns>
    public static int FindCodeSnippet(List<CodeInstruction>      pCodes, out List<CodeInstruction> pResult,
                                      params BaseInstPredictor[] pSnippetPredictors)
    {
        for (var i = 0; i < pCodes.Count - pSnippetPredictors.Length; i++)
            if (!pSnippetPredictors.Where((t, j) => !t.Predict(pCodes[i + j])).Any())
            {
                pResult = pCodes.GetRange(i, pSnippetPredictors.Length);
                return i;
            }

        pResult = null;
        return -1;
    }

    /// <summary>
    /// </summary>
    /// <param name="pCodes"></param>
    /// <param name="pSnippetPredictors"></param>
    /// <returns>Index of the start of the code snippet in <paramref name="pCodes" /></returns>
    public static int FindCodeSnippetIdx(List<CodeInstruction> pCodes, params BaseInstPredictor[] pSnippetPredictors)
    {
        for (var i = 0; i < pCodes.Count - pSnippetPredictors.Length; i++)
            if (!pSnippetPredictors.Where((t, j) => !t.Predict(pCodes[i + j])).Any())
                return i;
        return -1;
    }

    /// <summary>
    /// </summary>
    /// <param name="pCodes"></param>
    /// <param name="pPredictor"></param>
    /// <returns>First of expected code instruction</returns>
    public static CodeInstruction FindInst(List<CodeInstruction> pCodes, BaseInstPredictor pPredictor)
    {
        return pCodes.FirstOrDefault(pPredictor.Predict);
    }

    /// <summary>
    /// </summary>
    /// <param name="pCodes"></param>
    /// <param name="pPredictor"></param>
    /// <typeparam name="TOperand"></typeparam>
    /// <returns></returns>
    public static TOperand FindInstOperand<TOperand>(List<CodeInstruction> pCodes, BaseInstPredictor pPredictor)
    {
        CodeInstruction inst = FindInst(pCodes, pPredictor);
        if (inst == null) return default;
        return inst.operand is TOperand operand ? operand : default;
    }

    /// <summary>
    /// </summary>
    /// <param name="pCodes"></param>
    /// <param name="pPredictor"></param>
    /// <typeparam name="TOperand"></typeparam>
    /// <returns>Index of the first of expected code instruction</returns>
    public static int FindInstIdx<TOperand>(List<CodeInstruction> pCodes, BaseInstPredictor pPredictor)
    {
        for (var i = 0; i < pCodes.Count; i++)
            if (pPredictor.Predict(pCodes[i]))
                return i;

        return -1;
    }

    internal static void _init()
    {
        BaseInstPredictor._init();
    }
}