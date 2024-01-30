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
    }

    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
    public static class HauntMenuMinigameSetFilterTextPatch
    {
        public static bool Prefix(HauntMenuMinigame __instance)
        {
            if (__instance.HauntTarget != null && Options.GhostCanSeeOtherRoles.GetBool())
            {
                // 役職表示をカスタムロール名で上書き
                __instance.FilterText.text = Utils.GetDisplayRoleName(PlayerControl.LocalPlayer, __instance.HauntTarget);
                return false;
            }
            return true;
        }
    }
}
