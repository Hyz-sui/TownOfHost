using HarmonyLib;

namespace TownOfHost.Patches
{
    [HarmonyPatch]
    public static class HauntMenuMinigamePatch
    {
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(HauntMenuMinigame __instance)
        {
            if (Main.HauntMenuFocusCrewmate.Value && __instance.filterMode != HauntMenuMinigame.HauntFilters.Crewmate)
            {
                __instance.SetFilter((int)HauntMenuMinigame.HauntFilters.Crewmate);
            }
        }

        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText)), HarmonyPrefix]
        public static bool SetFilterTextPrefix(HauntMenuMinigame __instance)
        {
            __instance.FilterText.text = __instance.HauntTarget.GetTrueRoleName();

            return false;
        }
    }
}
