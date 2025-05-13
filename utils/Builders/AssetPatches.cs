using HarmonyLib;
using NeoModLoader.utils.Builders;
using System.Reflection.Emit;
using static NeoModLoader.utils.Builders.ActorTraitBuilder;

namespace NeoModLoader.utils
{
    internal class AssetPatches
    {
        [HarmonyPatch(typeof(Actor), "updateStats")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MergeWithCustomStats(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher Matcher = new CodeMatcher(instructions);
            Matcher.MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(BaseStats), nameof(BaseStats.clear)))
            });
            Matcher.Advance(1);
            Matcher.Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AssetPatches), nameof(MergeCustomStats)))
            });
            return Matcher.Instructions();
        }
        static void MergeCustomStats(Actor __instance)
        {
            foreach(ActorTrait trait in __instance.traits)
            {
                if(AdditionalBaseStatMethods.TryGetValue(trait.id, out GetAdditionalBaseStatsMethod method))
                {
                    __instance.stats.mergeStats(method(__instance));
                }
            }
        }
        static BaseStats[] GetCustomStats(ActorTrait trait)
        {
            if(SelectedUnit.unit == null || !SelectedUnit.unit.hasTrait(trait))
            {
                return Array.Empty<BaseStats>();
            }
            if(!AdditionalBaseStatMethods.TryGetValue(trait.id, out GetAdditionalBaseStatsMethod method))
            {
                return Array.Empty<BaseStats>();
            }
            return new BaseStats[] { method(SelectedUnit.unit) };
        }
        [HarmonyPatch(typeof(TooltipLibrary), "showTrait")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ShowCustomStats(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher Matcher = new CodeMatcher(instructions);
            Matcher.MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(OpCodes.Call, AccessTools.Field(typeof(Array), nameof(Array.Empty)))
            });
            Matcher.RemoveInstruction();
            Matcher.Insert(new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AssetPatches), nameof(GetCustomStats))) });
            return Matcher.Instructions();
        }
    }
}
