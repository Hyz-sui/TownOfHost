using HarmonyLib;

using TownOfHost.Modules;
using TownOfHost.Modules.ClientOptions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {
        private static ClientActionItem ForceJapanese;
        private static ClientActionItem JapaneseRoleName;
        private static ClientActionItem UnloadMod;
        private static ClientActionItem DumpLog;
        private static ClientActionItem SendResultToDiscord;
        private static ClientActionItem SendHistoryToDiscord;
        private static ClientActionItem ShowLobbySummary;
        private static ClientActionItem CopyGameCodeOnCreateLobby;
        private static ClientActionItem HauntMenuFocusCrewmate;

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.DisableMouseMovement == null)
            {
                return;
            }

            if (ForceJapanese == null || ForceJapanese.ToggleButton == null)
            {
                ForceJapanese = ClientOptionItem.Create("ForceJapanese", Main.ForceJapanese, __instance);
            }
            if (JapaneseRoleName == null || JapaneseRoleName.ToggleButton == null)
            {
                JapaneseRoleName = ClientOptionItem.Create("JapaneseRoleName", Main.JapaneseRoleName, __instance);
            }
            if (SendResultToDiscord == null || SendResultToDiscord.ToggleButton == null)
            {
                SendResultToDiscord = ClientOptionItem.Create("DiscordResult", Main.SendResultToDiscord, __instance);
            }
            if (SendHistoryToDiscord == null || SendHistoryToDiscord.ToggleButton == null)
            {
                SendHistoryToDiscord = ClientOptionItem.Create("DiscordHistory", Main.SendHistoryToDiscord, __instance);
            }
            if (ShowLobbySummary == null || ShowLobbySummary.ToggleButton == null)
            {
                ShowLobbySummary = ClientOptionItem.Create("ShowLobbySummary", Main.ShowLobbySummary, __instance, () =>
                {
                    if (DestroyableSingleton<GameStartManager>.InstanceExists)
                    {
                        LobbySummary.Show();
                    }
                });
            }
            if (CopyGameCodeOnCreateLobby == null || CopyGameCodeOnCreateLobby.ToggleButton == null)
            {
                CopyGameCodeOnCreateLobby = ClientOptionItem.Create("CopyCode", Main.CopyGameCodeOnCreateLobby, __instance);
            }
            if (HauntMenuFocusCrewmate == null || HauntMenuFocusCrewmate.ToggleButton == null)
            {
                HauntMenuFocusCrewmate = ClientOptionItem.Create("HauntFocusCrew", Main.HauntMenuFocusCrewmate, __instance);
            }
            if (UnloadMod == null || UnloadMod.ToggleButton == null)
            {
                UnloadMod = ClientActionItem.Create("UnloadMod", ModUnloaderScreen.Show, __instance);
            }
            if (DumpLog == null || DumpLog.ToggleButton == null)
            {
                DumpLog = ClientActionItem.Create("DumpLog", Utils.DumpLog, __instance);
            }

            if (ModUnloaderScreen.Popup == null)
            {
                ModUnloaderScreen.Init(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
    public static class OptionsMenuBehaviourClosePatch
    {
        public static void Postfix()
        {
            if (ClientActionItem.CustomBackground != null)
            {
                ClientActionItem.CustomBackground.gameObject.SetActive(false);
            }
            ModUnloaderScreen.Hide();
        }
    }
}
