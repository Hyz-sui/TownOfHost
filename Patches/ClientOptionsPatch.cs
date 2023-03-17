using HarmonyLib;

using TownOfHost.Objects;

namespace TownOfHost
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    class OptionsMenuBehaviourStartPatch
    {
        private static ClientOptionToggleButton ForceJapanese;
        private static ClientOptionToggleButton JapaneseRoleName;
        private static ClientOptionToggleButton SendResultToDiscord;
        private static ClientOptionToggleButton ShowLobbySummary;

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.DisableMouseMovement == null)
            {
                return;
            }

            if (ForceJapanese == null || ForceJapanese.Behaviour == null)
            {
                ForceJapanese = ClientOptionToggleButton.Create(
                    "日本語表示を強制",
                    "ForceJapanese",
                    Main.ForceJapanese,
                    __instance);
            }
            if (JapaneseRoleName == null || JapaneseRoleName.Behaviour == null)
            {
                JapaneseRoleName = ClientOptionToggleButton.Create(
                    "役職名を日本語で表示",
                    "JapaneseRoleName",
                    Main.JapaneseRoleName,
                    __instance);
            }
            if (SendResultToDiscord == null || SendResultToDiscord.Behaviour == null)
            {
                SendResultToDiscord = ClientOptionToggleButton.Create(
                    "Discordに試合結果を送信",
                    "DiscordResult",
                    Main.SendResultToDiscord,
                    __instance);
            }
            if (ShowLobbySummary == null || ShowLobbySummary.Behaviour == null)
            {
                ShowLobbySummary = ClientOptionToggleButton.Create(
                    "ロビーで前の試合の結果を表示",
                    "ShowLobbySummary",
                    Main.ShowLobbySummary,
                    __instance,
                    () =>
                    {
                        if (DestroyableSingleton<GameStartManager>.InstanceExists)
                        {
                            LobbySummary.Show();
                        }
                    });
            }
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
    public static class OptionsMenuBehaviourClosePatch
    {
        public static void Postfix()
        {
            ClientOptionToggleButton.CustomBackground?.gameObject?.SetActive(false);
        }
    }
}
